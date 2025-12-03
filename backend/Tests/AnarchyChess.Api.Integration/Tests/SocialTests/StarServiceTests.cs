using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Social.Errors;
using AnarchyChess.Api.Social.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.SocialTests;

public class StarServiceTests : BaseIntegrationTest
{
    private readonly IStarService _starService;

    public StarServiceTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _starService = Scope.ServiceProvider.GetRequiredService<IStarService>();
    }

    [Fact]
    public async Task IsStarredAsync_returns_true_if_user_has_starred_another()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        await _starService.AddStarAsync(user1.Id, user2.Id, CT);
        var result = await _starService.HasStarredAsync(user1.Id, user2.Id, CT);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsStarredAsync_returns_false_if_user_has_not_starred_another()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _starService.HasStarredAsync(user1.Id, user2.Id, CT);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetStarsOfAsync_applies_pagination()
    {
        var user = new AuthedUserFaker().Generate();
        var starredUsers = new AuthedUserFaker().Generate(5);

        await DbContext.AddAsync(user, CT);
        await DbContext.AddRangeAsync(starredUsers, CT);
        await DbContext.SaveChangesAsync(CT);

        foreach (var starred in starredUsers)
        {
            await _starService.AddStarAsync(user.Id, starred.Id, CT);
        }

        var result = await _starService.GetStarredUsersAsync(
            user.Id,
            new PaginationQuery(Page: 1, PageSize: 2),
            CT
        );

        result.Items.Should().HaveCount(2);
        result
            .Items.Should()
            .BeEquivalentTo(starredUsers.Skip(2).Take(2).Select(x => new MinimalProfile(x)));
        result.TotalCount.Should().Be(starredUsers.Count);
    }

    [Fact]
    public async Task AddStarAsync_creates_new_star_if_none_exists()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _starService.AddStarAsync(user1.Id, user2.Id, CT);

        result.IsError.Should().BeFalse();

        var dbStar = await DbContext.StarredUsers.AsNoTracking().SingleOrDefaultAsync(CT);
        dbStar.Should().NotBeNull();
        dbStar.UserId.Should().Be(user1.Id);
        dbStar.StarredUserId.Should().Be(user2.Id);
    }

    [Fact]
    public async Task AddStarAsync_returns_error_if_already_starred()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        await _starService.AddStarAsync(user1.Id, user2.Id, CT);

        var result = await _starService.AddStarAsync(user1.Id, user2.Id, CT);

        result.FirstError.Should().Be(SocialErrors.AlreadyStarred);
    }

    [Fact]
    public async Task AddStartAsync_returns_error_when_trying_to_star_yourself()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _starService.AddStarAsync(user.Id, user.Id, CT);

        result.FirstError.Should().Be(SocialErrors.CannotStarSelf);
        var dbStars = await DbContext.StarredUsers.AsNoTracking().ToListAsync(CT);
        dbStars.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveStarAsync_deletes_star_if_exists()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        await _starService.AddStarAsync(user1.Id, user2.Id, CT);

        var result = await _starService.RemoveStarAsync(user1.Id, user2.Id, CT);

        result.IsError.Should().BeFalse();

        var dbStar = await DbContext.StarredUsers.AsNoTracking().SingleOrDefaultAsync(CT);
        dbStar.Should().BeNull();
    }

    [Fact]
    public async Task RemoveStarAsync_returns_error_if_star_does_not_exist()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _starService.RemoveStarAsync(user1.Id, user2.Id, CT);

        result.FirstError.Should().Be(SocialErrors.NotStarred);
    }
}
