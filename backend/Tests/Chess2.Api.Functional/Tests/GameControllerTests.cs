using System.Net;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Functional.Tests;

public class GameControllerTests : BaseFunctionalTest
{
    private readonly ILiveGameService _gameService;

    public GameControllerTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameService = Scope.ServiceProvider.GetRequiredService<ILiveGameService>();
    }

    [Fact]
    public async Task GetGame_returns_game_state_for_guest_player()
    {
        var gameToken = await _gameService.StartGameAsync(
            "guest1",
            "guest2",
            new TimeControlSettings(600, 0),
            isRated: false
        );
        AuthUtils.AuthenticateGuest(ApiClient, "guest1");

        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();
        AssertGuestPlayersMatch("guest1", "guest2", gameState);
    }

    [Fact]
    public async Task GetGame_returns_game_state_for_authed_player()
    {
        var (user1, user1Rating, user2, user2Rating, gameToken) = await CreateRatedGameAsync();

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user1);

        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();
        AssertAuthedPlayersMatch(user1, user1Rating, user2, user2Rating, gameState);
    }

    [Fact]
    public async Task GetGame_returns_403_for_guest_not_in_game()
    {
        var gameToken = await _gameService.StartGameAsync(
            "guest1",
            "guest2",
            new(600, 0),
            isRated: false
        );
        AuthUtils.AuthenticateGuest(ApiClient, "otherGuest");

        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetGame_returns_403_for_authed_user_not_in_game()
    {
        var gameToken = await _gameService.StartGameAsync(
            "guest1",
            "guest2",
            new(600, 0),
            isRated: false
        );

        // authenticate with a different user
        await AuthUtils.AuthenticateAsync(ApiClient);
        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetGame_returns_404_for_invalid_game_token()
    {
        var token = TokenProvider.GenerateGuestToken("guest1");
        AuthUtils.AuthenticateWithTokens(ApiClient, token);

        var response = await ApiClient.Api.GetGameAsync("thisgamedoesnotexist123");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetGame_returns_401_for_unauthenticated_user()
    {
        var response = await ApiClient.Api.GetGameAsync("anygametoken");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGame_returns_correct_game_after_it_is_over_when_rated()
    {
        var (user1, user1Rating, user2, user2Rating, gameToken) = await CreateRatedGameAsync();

        await _gameService.EndGameAsync(gameToken, user2.Id, CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user1);
        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();

        gameState.ResultData.Should().NotBeNull();
        gameState.LegalMoves.Should().BeEmpty();
        AssertAuthedPlayersMatch(user1, user1Rating, user2, user2Rating, gameState);
    }

    [Fact]
    public async Task GetGame_returns_correct_game_after_it_is_over_when_unrated()
    {
        var gameToken = await _gameService.StartGameAsync(
            "guest1",
            "guest2",
            new TimeControlSettings(600, 0),
            isRated: false
        );
        await _gameService.EndGameAsync(gameToken, "guest2", CT);

        AuthUtils.AuthenticateGuest(ApiClient, "guest1");
        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();

        gameState.ResultData.Should().NotBeNull();
        gameState.LegalMoves.Should().BeEmpty();
        AssertGuestPlayersMatch("guest1", "guest2", gameState);
    }

    [Fact]
    public async Task GetGame_returns_game_state_for_mixed_player_types()
    {
        // Arrange: create one authed user and use one guest ID
        var authedUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var guestId = "guest123";

        var rating = await FakerUtils.StoreFakerAsync(
            DbContext,
            new CurrentRatingFaker(authedUser, 1500).RuleFor(x => x.TimeControl, TimeControl.Blitz)
        );

        var gameToken = await _gameService.StartGameAsync(
            authedUser.Id,
            guestId,
            new TimeControlSettings(300, 5),
            isRated: false
        );

        await AssertMixedPlayersGameState(authedUser, rating, guestId, gameToken);
    }

    private async Task AssertMixedPlayersGameState(
        AuthedUser authedUser,
        CurrentRating authedRating,
        string guestId,
        string gameToken
    )
    {
        await AuthUtils.AuthenticateWithUserAsync(ApiClient, authedUser);
        var responseAsAuthed = await ApiClient.Api.GetGameAsync(gameToken);

        responseAsAuthed.StatusCode.Should().Be(HttpStatusCode.OK);
        var gameState = responseAsAuthed.Content;
        gameState.Should().NotBeNull();

        var players = new[] { gameState.WhitePlayer, gameState.BlackPlayer };

        var authedPlayer = players.First(p => p.UserId == authedUser.Id);
        authedPlayer.UserName.Should().Be(authedUser.UserName);
        authedPlayer.CountryCode.Should().Be(authedUser.CountryCode);
        authedPlayer.Rating.Should().Be(authedRating.Value);

        var guestPlayer = players.First(p => p.UserId == guestId);
        guestPlayer.UserName.Should().Be("Guest");
        guestPlayer.CountryCode.Should().BeNull();
        guestPlayer.Rating.Should().BeNull();
    }

    private async Task<(
        AuthedUser user1,
        CurrentRating user1Rating,
        AuthedUser user2,
        CurrentRating user2Rating,
        string gameToken
    )> CreateRatedGameAsync()
    {
        var timeControl = new TimeControlSettings(30, 0);
        var user1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var user2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var user1Rating = await FakerUtils.StoreFakerAsync(
            DbContext,
            new CurrentRatingFaker(user1, 1200).RuleFor(x => x.TimeControl, TimeControl.Bullet)
        );
        var user2Rating = await FakerUtils.StoreFakerAsync(
            DbContext,
            new CurrentRatingFaker(user2, 1300).RuleFor(x => x.TimeControl, TimeControl.Bullet)
        );

        var gameToken = await _gameService.StartGameAsync(
            user1.Id,
            user2.Id,
            timeControl,
            isRated: true
        );

        return (user1, user1Rating, user2, user2Rating, gameToken);
    }

    private static void AssertAuthedPlayersMatch(
        AuthedUser user1,
        CurrentRating user1Rating,
        AuthedUser user2,
        CurrentRating user2Rating,
        GameState gameState
    )
    {
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

    private static void AssertGuestPlayersMatch(
        string guest1Id,
        string guest2Id,
        GameState gameState
    )
    {
        var players = new[] { gameState.WhitePlayer, gameState.BlackPlayer };
        players.Select(p => p.UserId).Should().BeEquivalentTo([guest1Id, guest2Id]);
        players.Should().OnlyContain(p => p.UserName == "Guest");
        players.Should().OnlyContain(p => p.CountryCode == null);
        players.Should().OnlyContain(p => p.Rating == null);
    }
}
