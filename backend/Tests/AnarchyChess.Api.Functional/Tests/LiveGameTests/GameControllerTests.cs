using System.Net;
using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AnarchyChess.Api.UserRating.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Functional.Tests.LiveGameTests;

public class GameControllerTests : BaseFunctionalTest
{
    private readonly IGameStarter _gameStarter;
    private readonly IGrainFactory _grains;

    public GameControllerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _gameStarter = Scope.ServiceProvider.GetRequiredService<IGameStarter>();
        _grains = Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
    }

    [Fact]
    public async Task GetGame_returns_game_state_for_guest_player()
    {
        var guest1 = UserId.Guest();
        var guest2 = UserId.Guest();
        var gameToken = await _gameStarter.StartGameAsync(
            guest1,
            guest2,
            new PoolKey(PoolType.Casual, new(600, 0)),
            token: CT
        );
        AuthUtils.AuthenticateGuest(ApiClient, guest1);

        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.IsSuccessful.Should().BeTrue();

        var gameState = response.Content;
        gameState.Should().NotBeNull();
        AssertGuestPlayersMatch(guest1, guest2, gameState);
    }

    [Fact]
    public async Task GetGame_returns_game_state_for_authed_player()
    {
        var startGame = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, startGame.User1);

        var response = await ApiClient.Api.GetGameAsync(startGame.GameToken);

        response.IsSuccessful.Should().BeTrue();

        var gameState = response.Content;
        gameState.Should().NotBeNull();
        AssertAuthedPlayersMatch(
            startGame.User1,
            startGame.User1Rating,
            startGame.User2,
            startGame.User2Rating,
            gameState
        );
    }

    [Fact]
    public async Task GetGame_returns_gane_state_for_guest_not_in_game()
    {
        var gameToken = await _gameStarter.StartGameAsync(
            UserId.Guest(),
            UserId.Guest(),
            new PoolKey(PoolType.Casual, new(600, 0)),
            token: CT
        );
        AuthUtils.AuthenticateGuest(ApiClient, UserId.Guest());

        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.IsSuccessful.Should().BeTrue();

        response.Content.Should().NotBeNull();
        response.Content.MoveOptions.Should().BeEquivalentTo(new MoveOptions());
    }

    [Fact]
    public async Task GetGame_returns_game_state_for_authed_user_not_in_game()
    {
        var gameToken = await _gameStarter.StartGameAsync(
            UserId.Guest(),
            UserId.Guest(),
            new PoolKey(PoolType.Casual, new(600, 0)),
            token: CT
        );

        // authenticate with a different user
        await AuthUtils.AuthenticateAsync(ApiClient);
        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.IsSuccessful.Should().BeTrue();

        response.Content.Should().NotBeNull();
        response.Content.MoveOptions.Should().BeEquivalentTo(new MoveOptions());
    }

    [Fact]
    public async Task GetGame_returns_404_for_invalid_game_token()
    {
        AuthUtils.AuthenticateGuest(ApiClient);

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
        var startGame = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);
        await _grains
            .GetGrain<IGameGrain>(startGame.GameToken)
            .RequestGameEndAsync(startGame.User2.Id, CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, startGame.User1);
        var response = await ApiClient.Api.GetGameAsync(startGame.GameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();

        gameState.ResultData.Should().NotBeNull();
        gameState
            .MoveOptions.Should()
            .BeEquivalentTo(new MoveOptions(LegalMoves: [], HasForcedMoves: false));
        AssertAuthedPlayersMatch(
            startGame.User1,
            startGame.User1Rating,
            startGame.User2,
            startGame.User2Rating,
            gameState
        );
    }

    [Fact]
    public async Task GetGame_returns_correct_game_after_it_is_over_when_unrated()
    {
        var guest1 = UserId.Guest();
        var guest2 = UserId.Guest();
        var gameToken = await _gameStarter.StartGameAsync(
            guest1,
            guest2,
            new PoolKey(PoolType.Casual, new(600, 0)),
            token: CT
        );
        await _grains.GetGrain<IGameGrain>(gameToken).RequestGameEndAsync(guest2, CT);

        AuthUtils.AuthenticateGuest(ApiClient, guest1);
        var response = await ApiClient.Api.GetGameAsync(gameToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = response.Content;
        gameState.Should().NotBeNull();

        gameState.ResultData.Should().NotBeNull();
        gameState
            .MoveOptions.Should()
            .BeEquivalentTo(new MoveOptions(LegalMoves: [], HasForcedMoves: false));
        AssertGuestPlayersMatch(guest1, guest2, gameState);
    }

    [Fact]
    public async Task GetGame_returns_game_state_for_mixed_player_types()
    {
        var authedUser = new AuthedUserFaker().Generate();
        var rating = new CurrentRatingFaker(
            authedUser,
            1500,
            timeControl: TimeControl.Blitz
        ).Generate();
        await DbContext.AddRangeAsync(authedUser, rating);
        await DbContext.SaveChangesAsync(CT);

        var guestId = UserId.Guest();
        var gameToken = await _gameStarter.StartGameAsync(
            authedUser.Id,
            guestId,
            new PoolKey(PoolType.Casual, new(300, 5)),
            token: CT
        );

        await AssertMixedPlayersGameState(authedUser, rating, guestId, gameToken);
    }

    [Fact]
    public async Task GetGameResults_ReturnsExpectedResults()
    {
        UserId userId = "test-user";
        var archives = new GameArchiveFaker(whiteUserId: userId)
            .RuleFor(x => x.Result, f => f.PickRandomWithout(GameResult.Aborted))
            .Generate(3);
        var otherArchives = new GameArchiveFaker(whiteUserId: "someone else")
            .RuleFor(x => x.Result, f => f.PickRandomWithout(GameResult.Aborted))
            .Generate(3);
        await DbContext.GameArchives.AddRangeAsync(archives.Concat(otherArchives), CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetGameResultsAsync(userId, new(Page: 0, PageSize: 2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = response.Content;
        content.Should().NotBeNull();
        content.Page.Should().Be(0);
        content.PageSize.Should().Be(2);
        content.TotalCount.Should().Be(3);
        content
            .Items.Should()
            .OnlyContain(game => game.WhitePlayer.UserId == userId)
            .And.HaveCount(2);
    }

    [Fact]
    public async Task GetGameResults_ReturnsEmptyResult_WhenUserHasNoArchives()
    {
        var response = await ApiClient.Api.GetGameResultsAsync(
            "non existing",
            new(Page: 0, PageSize: 5)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = response.Content;
        content.Should().NotBeNull();
        content.Page.Should().Be(0);
        content.PageSize.Should().Be(5);
        content.TotalCount.Should().Be(0);
        content.Items.Should().BeEmpty();
    }

    private async Task AssertMixedPlayersGameState(
        AuthedUser authedUser,
        CurrentRating authedRating,
        UserId guestId,
        GameToken gameToken
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
        guestPlayer.CountryCode.Should().Be("XX");
        guestPlayer.Rating.Should().BeNull();
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
        UserId guest1Id,
        UserId guest2Id,
        GameState gameState
    )
    {
        var players = new[] { gameState.WhitePlayer, gameState.BlackPlayer };
        players.Select(p => p.UserId).Should().BeEquivalentTo([guest1Id, guest2Id]);
        players.Should().OnlyContain(p => p.UserName == "Guest");
        players.Should().OnlyContain(p => p.CountryCode == "XX");
        players.Should().OnlyContain(p => p.Rating == null);
    }
}
