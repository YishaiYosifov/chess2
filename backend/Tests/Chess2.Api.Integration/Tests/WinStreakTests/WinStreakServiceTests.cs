using Chess2.Api.Game.Models;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Streaks.Entities;
using Chess2.Api.Streaks.Models;
using Chess2.Api.Streaks.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.WinStreakTests;

public class WinStreakServiceTests : BaseIntegrationTest
{
    private readonly IWinStreakService _service;

    public WinStreakServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _service = Scope.ServiceProvider.GetRequiredService<IWinStreakService>();
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_applies_pagination()
    {
        List<UserWinStreak> streaks =
        [
            new UserWinStreakFaker(highestStreak: 4).Generate(),
            new UserWinStreakFaker(highestStreak: 3).Generate(),
            new UserWinStreakFaker(highestStreak: 2).Generate(),
            new UserWinStreakFaker(highestStreak: 1).Generate(),
        ];
        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 3);

        var result = await _service.GetPaginatedLeaderboardAsync(pagination, CT);

        var expected = streaks[..3].Select(x => new WinStreakDto(x)).ToList();
        result.Items.Should().BeEquivalentTo(expected);
        result.TotalCount.Should().Be(streaks.Count);
        result.Page.Should().Be(pagination.Page);
        result.PageSize.Should().Be(pagination.PageSize);
    }

    [Fact]
    public async Task GetMyStatsAsync_finds_correct_stats()
    {
        var streaks = new UserWinStreakFaker().Generate(5);
        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.SaveChangesAsync(CT);

        var streakToTest = streaks[2];
        var result = await _service.GetMyStatsAsync(streakToTest.UserId, CT);

        MyWinStreakStats expectedRank = new(
            Rank: streaks.Count(u =>
                u.HighestStreakGames.Count > streakToTest.HighestStreakGames.Count
            ) + 1,
            HighestStreak: streakToTest.HighestStreakGames.Count,
            CurrentStreak: streakToTest.CurrentStreakGames.Count
        );
        result.Should().Be(expectedRank);
    }

    [Fact]
    public async Task GetMyStatsAsync_finds_correct_stats_when_no_streak_is_recorded()
    {
        var streaks = new UserWinStreakFaker().Generate(5);
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _service.GetMyStatsAsync(user.Id, CT);

        MyWinStreakStats expectedRank = new(
            Rank: streaks.Count + 1,
            HighestStreak: 0,
            CurrentStreak: 0
        );
        result.Should().Be(expectedRank);
    }

    [Fact]
    public async Task IncrementStreakAsync_adds_streak_when_no_streak_exists()
    {
        GameToken gameToken = "test game token";
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        await _service.IncrementStreakAsync(user, gameToken, CT);

        var inDb = await DbContext.WinStreaks.AsNoTracking().SingleAsync(CT);
        UserWinStreak expectedStreak = new()
        {
            UserId = user.Id,
            User = user,
            CurrentStreakGames = [gameToken],
            HighestStreakGames = [gameToken],
        };
        inDb.Should().BeEquivalentTo(expectedStreak);
    }

    [Fact]
    public async Task IncrementStreakAsync_increments_streak_by_one_when_exists()
    {
        GameToken gameToken = "test game token";
        var streak = new UserWinStreakFaker(currentStreak: 5, highestStreak: 10).Generate();
        await DbContext.AddAsync(streak, CT);
        await DbContext.SaveChangesAsync(CT);

        await _service.IncrementStreakAsync(streak.User, gameToken, CT);

        var inDb = await DbContext.WinStreaks.AsNoTracking().SingleAsync(CT);
        streak.CurrentStreakGames.Add(gameToken);
        inDb.Should().BeEquivalentTo(inDb);
    }

    [Fact]
    public async Task IncrementStreakAsync_sets_highest_streak_when_current_streak_exceeds()
    {
        GameToken gameToken = "test game token";
        var streak = new UserWinStreakFaker(currentStreak: 3, highestStreak: 3).Generate();
        await DbContext.AddAsync(streak, CT);
        await DbContext.SaveChangesAsync(CT);

        await _service.IncrementStreakAsync(streak.User, gameToken, CT);

        var inDb = await DbContext.WinStreaks.AsNoTracking().SingleAsync(CT);
        streak.CurrentStreakGames.Add(gameToken);
        streak.HighestStreakGames.Add(gameToken);
        inDb.Should().BeEquivalentTo(inDb);
    }

    [Fact]
    public async Task EndStreakAsync_sets_streaks_to_zero()
    {
        var streak = new UserWinStreakFaker(currentStreak: 10, highestStreak: 10).Generate();
        await DbContext.AddAsync(streak, CT);
        await DbContext.SaveChangesAsync(CT);

        await _service.EndStreakAsync(streak.UserId, CT);

        var inDb = await DbContext.WinStreaks.AsNoTracking().SingleAsync(CT);
        streak.CurrentStreakGames = [];
        inDb.Should().BeEquivalentTo(streak);
    }
}
