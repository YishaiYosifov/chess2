using System.Net;
using Chess2.Api.Game.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.GameFlowTests;

public class GameControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalGameFlowTests(factory)
{
    [Fact]
    public async Task GetLiveGame_returns_game_state_for_guest_player()
    {
        var timeControl = new TimeControlSettings(600, 0);
        var (gameToken, accessToken, _) = await StartGuestMatchAsync(
            "guest1",
            "guest2",
            timeControl
        );
        AuthUtils.AuthenticateWithTokens(ApiClient, accessToken);

        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();

        var players = new[] { gameState.PlayerWhite, gameState.PlayerBlack };
        players.Select(p => p.UserId).Should().BeEquivalentTo(["guest1", "guest2"]);
        players.Should().OnlyContain(p => p.UserName == "Guest");
        players.Should().OnlyContain(p => p.CountryCode == null);
        players.Should().OnlyContain(p => p.Rating == null);
    }

    [Fact]
    public async Task GetLiveGame_returns_game_state_for_authed_player()
    {
        var user1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var user1Rating = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingFaker(user1, 1200).RuleFor(x => x.TimeControl, TimeControl.Bullet)
        );
        var user2Rating = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingFaker(user2, 1300).RuleFor(x => x.TimeControl, TimeControl.Bullet)
        );

        DbContext.Dispose();
        var (gameToken, accessToken, _) = await StartAuthedMatchAsync(user1, user2, new(30, 0));

        AuthUtils.AuthenticateWithTokens(ApiClient, accessToken);
        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();

        var players = new[] { gameState.PlayerWhite, gameState.PlayerBlack };
        var player1 = players.First(x => x.UserId == user1.Id);
        var player2 = players.First(x => x.UserId == user2.Id);

        player1.UserName.Should().Be(user1.UserName);
        player1.CountryCode.Should().Be(user1.CountryCode);
        player1.Rating.Should().Be(user1Rating.Value);

        player2.UserName.Should().Be(user2.UserName);
        player2.CountryCode.Should().Be(user2.CountryCode);
        player2.Rating.Should().Be(user2Rating.Value);
    }

    [Fact]
    public async Task GetLiveGame_returns_403_for_guest_not_in_game()
    {
        var (gameToken, _, _) = await StartGuestMatchAsync("guest1", "guest2", new(600, 0));
        var unrelatedToken = TokenProvider.GenerateGuestToken("otherGuest");
        AuthUtils.AuthenticateWithTokens(ApiClient, unrelatedToken);

        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLiveGame_returns_403_for_authed_user_not_in_game()
    {
        var (gameToken, _, _) = await StartGuestMatchAsync("guest1", "guest2", new(600, 0));

        // authenticate with a different user
        await AuthUtils.AuthenticateAsync(ApiClient);
        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLiveGame_returns_404_for_invalid_game_id()
    {
        var token = TokenProvider.GenerateGuestToken("guest1");
        AuthUtils.AuthenticateWithTokens(ApiClient, token);

        var response = await ApiClient.Api.GetLiveGameAsync("thisgamedoesnotexist123");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLiveGame_returns_401_for_unauthenticated_user()
    {
        var response = await ApiClient.Api.GetLiveGameAsync("anygameid");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
