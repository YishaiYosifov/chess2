using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Social.Repository;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.SocialTests;

public class BlockRepositoryTests : BaseIntegrationTest
{
    private readonly IBlockRepository _repository;

    public BlockRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IBlockRepository>();
    }

    [Fact]
    public async Task GetPaginatedBlockedUsersAsync_applies_pagination()
    {
        UserId userId = "test user";
        var blockedUsers = new BlockedUserFaker(forUser: userId).Generate(5);

        await DbContext.AddRangeAsync(blockedUsers, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPaginatedBlockedUsersAsync(
            userId,
            new PaginationQuery(Page: 1, PageSize: 2),
            CT
        );

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(blockedUsers.Skip(2).Take(2).Select(b => b.Blocked));
    }

    [Fact]
    public async Task GetAllBlockedUserIdsAsync_returns_only_user_blocked_ids()
    {
        UserId userId = "test-user";
        var blockedUsers = new BlockedUserFaker(forUser: userId).Generate(3);
        var otherBlocked = new BlockedUserFaker().Generate();

        await DbContext.AddRangeAsync(blockedUsers, CT);
        await DbContext.AddAsync(otherBlocked, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetAllBlockedUserIdsAsync(userId, CT);

        result.Should().BeEquivalentTo(blockedUsers.Select(b => b.BlockedUserId));
        result.Should().NotContain(otherBlocked.BlockedUserId);
    }

    [Fact]
    public async Task GetBlockedCountAsync_returns_correct_number()
    {
        UserId userId = "test user";
        var blockedUsers = new BlockedUserFaker(forUser: userId).Generate(4);
        var otherBlocked = new BlockedUserFaker().Generate();

        await DbContext.AddRangeAsync(blockedUsers, CT);
        await DbContext.AddRangeAsync(otherBlocked);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetBlockedCountAsync(userId, CT);

        result.Should().Be(blockedUsers.Count);
    }

    [Fact]
    public async Task GetBlockedUserAsync_returns_block_if_exists()
    {
        var blockedUser = new BlockedUserFaker().Generate();
        var other = new BlockedUserFaker().Generate();

        await DbContext.AddRangeAsync(blockedUser, other);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetBlockedUserAsync(
            blockedUser.UserId,
            blockedUser.BlockedUserId,
            CT
        );

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(blockedUser);
    }

    [Fact]
    public async Task GetBlockedUserAsync_returns_null_if_no_block_exists()
    {
        var blockedUser = new BlockedUserFaker().Generate();
        await DbContext.AddAsync(blockedUser, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetBlockedUserAsync(
            blockedUser.UserId,
            "nonexistent-user",
            CT
        );

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddBlockedUserAsync_adds_block()
    {
        var blockedUser = new BlockedUserFaker().Generate();

        await _repository.AddBlockedUserAsync(blockedUser, CT);
        await DbContext.SaveChangesAsync(CT);

        var dbBlock = await DbContext.BlockedUsers.AsNoTracking().SingleOrDefaultAsync(CT);

        dbBlock.Should().NotBeNull();
        dbBlock.Should().BeEquivalentTo(blockedUser);
    }

    [Fact]
    public async Task RemoveBlockedUser_removes_block_from_db_context()
    {
        var blockToDelete = new BlockedUserFaker().Generate();
        var otherBlock = new BlockedUserFaker().Generate();

        await DbContext.AddRangeAsync(blockToDelete, otherBlock);
        await DbContext.SaveChangesAsync(CT);

        _repository.RemoveBlockedUser(blockToDelete);
        await DbContext.SaveChangesAsync(CT);

        var dbBlocks = await DbContext.BlockedUsers.AsNoTracking().ToListAsync(CT);
        dbBlocks.Should().ContainSingle().Which.Should().BeEquivalentTo(otherBlock);
    }
}
