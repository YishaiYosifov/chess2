using System.Net;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Streaks.Entities;
using Chess2.Api.Streaks.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests;

public class WinStreakControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetWinStreakLeaderboard_returns_correct_leaderboard()
    {
        List<UserWinStreak> streaks =
        [
            new UserWinStreakFaker(highestStreak: 4),
            new UserWinStreakFaker(highestStreak: 3),
            new UserWinStreakFaker(highestStreak: 2),
            new UserWinStreakFaker(highestStreak: 1),
        ];
        await DbContext.AddRangeAsync(streaks, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 3);

        var response = await ApiClient.Api.GetWinStreakLeaderboardAsync(
            new PaginationQuery(Page: 0, PageSize: 3)
        );

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        response.Content.TotalCount.Should().Be(streaks.Count);
        response
            .Content.Items.Should()
            .BeEquivalentTo(streaks[..3].Select(x => new WinStreakDto(x)));
    }

    [Fact]
    public async Task GetMyWinStreakStats_returns_correct_rank()
    {
        var streak = new UserWinStreakFaker(highestStreak: 3).Generate();
        var higherStreak = new UserWinStreakFaker(highestStreak: 4).Generate(5);
        await DbContext.AddAsync(streak, CT);
        await DbContext.AddRangeAsync(higherStreak, CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, streak.User);

        var response = await ApiClient.Api.GetMyWinStreakStatsAsync();

        response.IsSuccessful.Should().BeTrue();
        MyWinStreakStats expectedRank = new(
            Rank: higherStreak.Count + 1,
            HighestStreak: streak.HighestStreakGames.Count,
            CurrentStreak: streak.CurrentStreakGames.Count
        );
        response.Content.Should().BeEquivalentTo(expectedRank);
    }

    [Fact]
    public async Task GetMyStreakRanking_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

        var response = await ApiClient.Api.GetMyWinStreakStatsAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
