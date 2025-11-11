using Chess2.Api.Pagination.Models;
using Chess2.Api.Streaks.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.WinStreakTests;

public class WinStreakRepositoryTests : BaseIntegrationTest
{
    private readonly IWinStreakRepository _repository;

    public WinStreakRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IWinStreakRepository>();
    }

    [Fact]
    public async Task AddAsync_adds_streak()
    {
        var streak = new UserWinStreakFaker().Generate();

        await _repository.AddAsync(streak, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.WinStreaks.AsNoTracking().ToListAsync(CT);
        inDb.Should().ContainSingle().Which.Should().BeEquivalentTo(streak);
    }

    [Fact]
    public async Task ClearCurrentStreakAsync_resets_current_streak_for_user()
    {
        var user1Streak = new UserWinStreakFaker(currentStreak: 5, highestStreak: 10).Generate();
        var user2Streak = new UserWinStreakFaker(currentStreak: 5).Generate();
        await DbContext.AddRangeAsync(user1Streak, user2Streak);
        await DbContext.SaveChangesAsync(CT);

        await _repository.ClearCurrentStreakAsync(user1Streak.UserId, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.WinStreaks.AsNoTracking().ToListAsync(CT);
        user1Streak.CurrentStreakGames = [];
        inDb.Should().BeEquivalentTo([user1Streak, user2Streak]);
    }

    [Fact]
    public async Task GetUserStreaksAsync_returns_streak_of_user()
    {
        var user1Streak = new UserWinStreakFaker().Generate();
        var user2Streak = new UserWinStreakFaker().Generate();
        await DbContext.AddRangeAsync(user1Streak, user2Streak);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetUserStreakAsync(user1Streak.UserId, CT);

        result.Should().BeEquivalentTo(user1Streak);
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_returns_correct_page_of_top_users()
    {
        int page = 1;
        int pageSize = 3;
        var streaks = new UserWinStreakFaker().Generate(10);
        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPaginatedLeaderboardAsync(
            new PaginationQuery(Page: page, PageSize: pageSize),
            CT
        );

        var expected = streaks
            .OrderByDescending(x => x.HighestStreakGames.Count)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        result.Should().HaveCount(expected.Count);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetTotalCountAsync_returns_the_number_of_users_with_streaks()
    {
        var questPoints = new UserWinStreakFaker().Generate(4);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTotalCountAsync(CT);
        result.Should().Be(questPoints.Count);
    }

    [Fact]
    public async Task GetRankingAsync_finds_user_position()
    {
        var streaks = new UserWinStreakFaker().Generate(5);

        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.SaveChangesAsync(CT);

        var streakToTest = streaks[2];
        var result = await _repository.GetRankingAsync(streakToTest.HighestStreakGames.Count, CT);

        result
            .Should()
            .Be(
                streaks.Count(x =>
                    x.HighestStreakGames.Count > streakToTest.HighestStreakGames.Count
                ) + 1
            );
    }
}
