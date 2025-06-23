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
        var playerIds = new[] { gameState.PlayerWhite.UserId, gameState.PlayerBlack.UserId };
        playerIds.Should().BeEquivalentTo(["guest1", "guest2"]);
    }

    [Fact]
    public async Task GetLiveGame_returns_game_state_for_authed_player()
    {
        var user1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var (gameToken, accessToken, _) = await StartAuthedMatchAsync(user1, user2, new(600, 0));

        AuthUtils.AuthenticateWithTokens(ApiClient, accessToken);
        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var gameState = response.Content;
        gameState.Should().NotBeNull();
        var playerIds = new[] { gameState.PlayerWhite.UserId, gameState.PlayerBlack.UserId };
        playerIds.Should().BeEquivalentTo([user1.Id, user2.Id]);
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
