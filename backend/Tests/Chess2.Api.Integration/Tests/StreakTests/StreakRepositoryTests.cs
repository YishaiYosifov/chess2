using Chess2.Api.Pagination.Models;
using Chess2.Api.Streaks.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.StreakTests;

public class StreakRepositoryTests : BaseIntegrationTest
{
    private readonly IStreakRepository _repository;

    public StreakRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IStreakRepository>();
    }

    [Fact]
    public async Task AddAsync_adds_streak()
    {
        var streak = new UserStreakFaker().Generate();

        await _repository.AddAsync(streak, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.Streaks.AsNoTracking().ToListAsync(CT);
        inDb.Should().ContainSingle().Which.Should().BeEquivalentTo(streak);
    }

    [Fact]
    public async Task ClearCurrentStreakAsync_resets_current_streak_for_user()
    {
        var user1Streak = new UserStreakFaker()
            .RuleFor(x => x.CurrentStreak, 5)
            .RuleFor(x => x.HighestStreak, 10)
            .Generate();
        var user2Streak = new UserStreakFaker().RuleFor(x => x.CurrentStreak, 5).Generate();
        await DbContext.AddRangeAsync(user1Streak, user2Streak);
        await DbContext.SaveChangesAsync(CT);

        await _repository.ClearCurrentStreakAsync(user1Streak.UserId, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.Streaks.AsNoTracking().ToListAsync(CT);
        user1Streak.CurrentStreak = 0;
        user1Streak.CurrentStreakGames = [];
        inDb.Should().BeEquivalentTo([user1Streak, user2Streak]);
    }

    [Fact]
    public async Task GetUserStreaksAsync_returns_streak_of_user()
    {
        var user1Streak = new UserStreakFaker().Generate();
        var user2Streak = new UserStreakFaker().Generate();
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
        var streaks = new UserStreakFaker().Generate(10);
        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPaginatedLeaderboardAsync(
            new PaginationQuery(Page: page, PageSize: pageSize),
            CT
        );

        var expected = streaks
            .OrderByDescending(x => x.HighestStreak)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        result.Should().HaveCount(expected.Count);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetTotalCountAsync_returns_the_number_of_users_with_streaks()
    {
        var questPoints = new UserStreakFaker().Generate(4);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTotalCountAsync(CT);
        result.Should().Be(questPoints.Count);
    }

    [Fact]
    public async Task GetRankingAsync_finds_user_position()
    {
        var streaks = new UserStreakFaker().Generate(5);

        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.SaveChangesAsync(CT);

        var streakToTest = streaks[2];
        var result = await _repository.GetRankingAsync(streakToTest.HighestStreak, CT);

        result.Should().Be(streaks.Count(x => x.HighestStreak > streakToTest.HighestStreak) + 1);
    }
}
