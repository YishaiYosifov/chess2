using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Social.Errors;
using Chess2.Api.Social.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.SocialTests;

public class BlockServiceTests : BaseIntegrationTest
{
    private readonly IBlockService _blockService;

    public BlockServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _blockService = Scope.ServiceProvider.GetRequiredService<IBlockService>();
    }

    [Fact]
    public async Task IsBlockedAsync_returns_true_if_user_has_blocked_another()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        await _blockService.BlockUserAsync(user1.Id, user2.Id, CT);
        var result = await _blockService.IsBlockedAsync(user1.Id, user2.Id, CT);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsBlockedAsync_returns_false_if_user_has_not_blocked_another()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _blockService.IsBlockedAsync(user1.Id, user2.Id, CT);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetBlockedUsersAsync_applies_pagination()
    {
        var user = new AuthedUserFaker().Generate();
        var blockedUsers = new AuthedUserFaker().Generate(5);

        await DbContext.AddAsync(user, CT);
        await DbContext.AddRangeAsync(blockedUsers, CT);
        await DbContext.SaveChangesAsync(CT);

        foreach (var blocked in blockedUsers)
        {
            await _blockService.BlockUserAsync(user.Id, blocked.Id, CT);
        }

        var result = await _blockService.GetBlockedUsersAsync(
            user.Id,
            new PaginationQuery(Page: 1, PageSize: 2),
            CT
        );

        result.Items.Should().HaveCount(2);
        result
            .Items.Should()
            .BeEquivalentTo(blockedUsers.Skip(2).Take(2).Select(x => new MinimalProfile(x)));
        result.TotalCount.Should().Be(blockedUsers.Count);
    }

    [Fact]
    public async Task BlockUserAsync_creates_new_block_if_none_exists()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _blockService.BlockUserAsync(user1.Id, user2.Id, CT);

        result.IsError.Should().BeFalse();

        var dbBlock = await DbContext.BlockedUsers.AsNoTracking().SingleOrDefaultAsync(CT);
        dbBlock.Should().NotBeNull();
        dbBlock.UserId.Should().Be(user1.Id);
        dbBlock.BlockedUserId.Should().Be(user2.Id);
    }

    [Fact]
    public async Task BlockUserAsync_returns_error_if_already_blocked()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        await _blockService.BlockUserAsync(user1.Id, user2.Id, CT);

        var result = await _blockService.BlockUserAsync(user1.Id, user2.Id, CT);

        result.FirstError.Should().Be(SocialErrors.AlreadyBlocked);
    }

    [Fact]
    public async Task BlockUserAsync_returns_error_when_trying_to_block_yourself()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _blockService.BlockUserAsync(user.Id, user.Id, CT);

        result.FirstError.Should().Be(SocialErrors.CannotBlockSelf);
        var dbBlocks = await DbContext.BlockedUsers.AsNoTracking().ToListAsync(CT);
        dbBlocks.Should().BeEmpty();
    }

    [Fact]
    public async Task UnblockUserAsync_deletes_block_if_exists()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        await _blockService.BlockUserAsync(user1.Id, user2.Id, CT);

        var result = await _blockService.UnblockUserAsync(user1.Id, user2.Id, CT);

        result.IsError.Should().BeFalse();

        var dbBlock = await DbContext.BlockedUsers.AsNoTracking().SingleOrDefaultAsync(CT);
        dbBlock.Should().BeNull();
    }

    [Fact]
    public async Task UnblockUserAsync_returns_error_if_block_does_not_exist()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _blockService.UnblockUserAsync(user1.Id, user2.Id, CT);

        result.FirstError.Should().Be(SocialErrors.NotBlocked);
    }
}
