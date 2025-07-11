using System.Net;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Functional.Tests;

public class GameControllerTests : BaseFunctionalTest
{
    private readonly IGameService _gameService;

    public GameControllerTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameService = Scope.ServiceProvider.GetRequiredService<IGameService>();
    }

    [Fact]
    public async Task GetLiveGame_returns_game_state_for_guest_player()
    {
        var timeControl = new TimeControlSettings(600, 0);
        var gameToken = await _gameService.StartGameAsync(
            "guest1",
            "guest2",
            timeControl,
            isRated: false
        );
        AuthUtils.AuthenticateGuest(ApiClient, "guest1");

        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();

        var players = new[] { gameState.WhitePlayer, gameState.BlackPlayer };
        players.Select(p => p.UserId).Should().BeEquivalentTo(["guest1", "guest2"]);
        players.Should().OnlyContain(p => p.UserName == "Guest");
        players.Should().OnlyContain(p => p.CountryCode == null);
        players.Should().OnlyContain(p => p.Rating == null);
    }

    [Fact]
    public async Task GetLiveGame_returns_game_state_for_authed_player()
    {
        var timeControl = new TimeControlSettings(30, 0);
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

        var gameToken = await _gameService.StartGameAsync(
            user1.Id,
            user2.Id,
            timeControl,
            isRated: true
        );
        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user1);

        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();

        var players = new[] { gameState.WhitePlayer, gameState.BlackPlayer };
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
        var gameToken = await _gameService.StartGameAsync(
            "guest1",
            "guest2",
            new(600, 0),
            isRated: false
        );
        AuthUtils.AuthenticateGuest(ApiClient, "otherGuest");

        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLiveGame_returns_403_for_authed_user_not_in_game()
    {
        var gameToken = await _gameService.StartGameAsync(
            "guest1",
            "guest2",
            new(600, 0),
            isRated: false
        );

        // authenticate with a different user
        await AuthUtils.AuthenticateAsync(ApiClient);
        var response = await ApiClient.Api.GetLiveGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLiveGame_returns_404_for_invalid_game_token()
    {
        var token = TokenProvider.GenerateGuestToken("guest1");
        AuthUtils.AuthenticateWithTokens(ApiClient, token);

        var response = await ApiClient.Api.GetLiveGameAsync("thisgamedoesnotexist123");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLiveGame_returns_401_for_unauthenticated_user()
    {
        var response = await ApiClient.Api.GetLiveGameAsync("anygametoken");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
