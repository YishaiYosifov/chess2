using Chess2.Api.Game.Models;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Streaks.Entities;
using Chess2.Api.Streaks.Models;
using Chess2.Api.Streaks.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.StreakTests;

public class StreakServiceTests : BaseIntegrationTest
{
    private readonly IStreakService _service;

    public StreakServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _service = Scope.ServiceProvider.GetRequiredService<IStreakService>();
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_applies_pagination()
    {
        List<UserStreak> streaks =
        [
            new UserStreakFaker().RuleFor(x => x.HighestStreak, 4).Generate(),
            new UserStreakFaker().RuleFor(x => x.HighestStreak, 3).Generate(),
            new UserStreakFaker().RuleFor(x => x.HighestStreak, 2).Generate(),
            new UserStreakFaker().RuleFor(x => x.HighestStreak, 1).Generate(),
        ];
        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 3);

        var result = await _service.GetPaginatedLeaderboardAsync(pagination, CT);

        var expected = streaks[..3]
            .Select(x => new StreakDto(
                new MinimalProfile(x.User),
                x.HighestStreak,
                [.. x.HighestStreakGames]
            ))
            .ToList();
        result.Items.Should().BeEquivalentTo(expected);
        result.TotalCount.Should().Be(streaks.Count);
        result.Page.Should().Be(pagination.Page);
        result.PageSize.Should().Be(pagination.PageSize);
    }

    [Fact]
    public async Task GetHighestStreakAsync_returns_user_highest_streak_when_exists()
    {
        var userStreak = new UserStreakFaker()
            .RuleFor(x => x.HighestStreak, 1000)
            .RuleFor(x => x.CurrentStreak, 500)
            .Generate();
        var otherUserStreak = new UserStreakFaker().RuleFor(x => x.HighestStreak, 500).Generate();
        await DbContext.AddRangeAsync(userStreak, otherUserStreak);
        await DbContext.SaveChangesAsync(CT);

        var result = await _service.GetHighestStreakAsync(userStreak.UserId, CT);

        result.Should().Be(userStreak.HighestStreak);
    }

    [Fact]
    public async Task GetHighestStreakAsync_returns_zero_when_no_streak_exists()
    {
        var otherUserStreak = new UserStreakFaker().RuleFor(x => x.HighestStreak, 500).Generate();
        await DbContext.AddAsync(otherUserStreak, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _service.GetHighestStreakAsync("non existing", CT);

        result.Should().Be(0);
    }

    [Fact]
    public async Task IncrementStreakAsync_adds_streak_when_no_streak_exists()
    {
        GameToken gameToken = "test game token";
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        await _service.IncrementStreakAsync(user, gameToken, CT);

        var inDb = await DbContext.Streaks.AsNoTracking().SingleAsync(CT);
        UserStreak expectedStreak = new()
        {
            UserId = user.Id,
            User = user,
            CurrentStreak = 1,
            CurrentStreakGames = [gameToken],
            HighestStreak = 1,
            HighestStreakGames = [gameToken],
        };
        inDb.Should().BeEquivalentTo(expectedStreak);
    }

    [Fact]
    public async Task IncrementStreakAsync_increments_streak_by_one_when_exists()
    {
        GameToken gameToken = "test game token";
        var streak = new UserStreakFaker()
            .RuleFor(x => x.CurrentStreak, 5)
            .RuleFor(x => x.HighestStreak, 10)
            .Generate();
        await DbContext.AddAsync(streak, CT);
        await DbContext.SaveChangesAsync(CT);

        await _service.IncrementStreakAsync(streak.User, gameToken, CT);

        var inDb = await DbContext.Streaks.AsNoTracking().SingleAsync(CT);
        streak.CurrentStreak++;
        streak.CurrentStreakGames.Add(gameToken);
        inDb.Should().BeEquivalentTo(inDb);
    }

    [Fact]
    public async Task IncrementStreakAsync_sets_highest_streak_when_current_streak_exceeds()
    {
        GameToken gameToken = "test game token";
        var streak = new UserStreakFaker()
            .RuleFor(x => x.CurrentStreak, 3)
            .RuleFor(x => x.HighestStreak, 3)
            .Generate();
        await DbContext.AddAsync(streak, CT);
        await DbContext.SaveChangesAsync(CT);

        await _service.IncrementStreakAsync(streak.User, gameToken, CT);

        var inDb = await DbContext.Streaks.AsNoTracking().SingleAsync(CT);
        streak.CurrentStreak++;
        streak.CurrentStreakGames.Add(gameToken);
        streak.HighestStreak++;
        streak.HighestStreakGames.Add(gameToken);
        inDb.Should().BeEquivalentTo(inDb);
    }

    [Fact]
    public async Task EndStreakAsync_sets_streaks_to_zero()
    {
        var streak = new UserStreakFaker()
            .RuleFor(x => x.CurrentStreak, 10)
            .RuleFor(x => x.HighestStreak, 10)
            .Generate();
        await DbContext.AddAsync(streak, CT);
        await DbContext.SaveChangesAsync(CT);

        await _service.EndStreakAsync(streak.UserId, CT);

        var inDb = await DbContext.Streaks.AsNoTracking().SingleAsync(CT);
        streak.CurrentStreak = 0;
        streak.CurrentStreakGames = [];
        inDb.Should().BeEquivalentTo(streak);
    }
}
