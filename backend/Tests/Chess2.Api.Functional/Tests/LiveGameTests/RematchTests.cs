using Chess2.Api.Game.Grains;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.Profile.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.SignalRClients;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Functional.Tests.LiveGameTests;

public class RematchTests : BaseFunctionalTest
{
    private readonly IGameStarter _gameStarter;
    private readonly IGrainFactory _grains;

    public RematchTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameStarter = Scope.ServiceProvider.GetRequiredService<IGameStarter>();
        _grains = Scope.ServiceProvider.GetRequiredService<IGrainFactory>();
    }

    [Fact]
    public async Task RequestRematchAsync_notifies_opponent()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);
        await FinishGameAsync(game.GameToken, game.User1.Id);

        await using var player1Conn = new GameHubClient(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User1),
            game.GameToken
        );
        await using var player2Conn = new GameHubClient(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User2),
            game.GameToken
        );

        await player1Conn.RequestRematchAsync(CT);

        await player2Conn.WaitForRematchRequestedAsync(CT);
    }

    [Fact]
    public async Task CancelRematchAsync_notifies_both_players()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);
        await FinishGameAsync(game.GameToken, game.User1.Id);

        await using var player1Conn = new GameHubClient(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User1),
            game.GameToken
        );
        await using var player2Conn = new GameHubClient(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User2),
            game.GameToken
        );

        await player1Conn.RequestRematchAsync(CT);
        await player2Conn.WaitForRematchRequestedAsync(CT);

        await player1Conn.CancelRematchAsync(CT);

        await player1Conn.WaitForRematchCancelledAsync(CT);
        await player2Conn.WaitForRematchCancelledAsync(CT);
    }

    [Fact]
    public async Task RequestRematchAsync_creates_game_when_both_are_requesting()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);
        await FinishGameAsync(game.GameToken, game.User1.Id);

        await using var player1Conn = new GameHubClient(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User1),
            game.GameToken
        );
        await using var player2Conn = new GameHubClient(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User2),
            game.GameToken
        );

        await player1Conn.RequestRematchAsync(CT);
        await player2Conn.RequestRematchAsync(CT);

        var newGameToken1 = await player1Conn.GetNextRematchAcceptedAsync(CT);
        var newGameToken2 = await player2Conn.GetNextRematchAcceptedAsync(CT);

        newGameToken1.Should().Be(newGameToken2);
        newGameToken1.Should().NotBe(game.GameToken);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, game.User1);
        var createdGameStateResult = await ApiClient.Api.GetGameAsync(newGameToken1);
        createdGameStateResult.IsSuccessful.Should().BeTrue();
        createdGameStateResult.Content.Should().NotBeNull();
        createdGameStateResult.Content.Pool.Should().Be(game.Pool);
        UserId[] playerIds =
        [
            createdGameStateResult.Content.WhitePlayer.UserId,
            createdGameStateResult.Content.BlackPlayer.UserId,
        ];
        playerIds.Should().BeEquivalentTo([game.User1.Id, game.User2.Id]);
    }

    [Fact]
    public async Task RequestRematchAsync_allows_guests()
    {
        var guest1 = UserId.Guest();
        var guest2 = UserId.Guest();

        var pool = new PoolKeyFaker().Generate();
        var gameToken = await _gameStarter.StartGameAsync(guest1, guest2, pool);
        await FinishGameAsync(gameToken, guest1);

        await using var guest1Conn = new GameHubClient(
            await GuestSignalRAsync(GameHubClient.Path(gameToken), guest1),
            gameToken
        );
        await using var guest2Conn = new GameHubClient(
            await GuestSignalRAsync(GameHubClient.Path(gameToken), guest2),
            gameToken
        );

        await guest1Conn.RequestRematchAsync(CT);

        await guest2Conn.WaitForRematchRequestedAsync(CT);

        await guest2Conn.RequestRematchAsync(CT);
        var newGameToken1 = await guest1Conn.GetNextRematchAcceptedAsync(CT);
        var newGameToken2 = await guest2Conn.GetNextRematchAcceptedAsync(CT);

        newGameToken1.Should().Be(newGameToken2);
        newGameToken1.Should().NotBe(gameToken);
    }

    [Fact]
    public async Task Disconnecting_player_tears_down_pending_rematch()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);
        await FinishGameAsync(game.GameToken, game.User1.Id);

        await using var player1Conn = new GameHubClient(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User1),
            game.GameToken
        );
        await using var player2Conn = new GameHubClient(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User2),
            game.GameToken
        );

        await player1Conn.RequestRematchAsync(CT);
        await player2Conn.WaitForRematchRequestedAsync(CT);

        await player1Conn.DisposeAsync();

        await player2Conn.WaitForRematchCancelledAsync(CT);
    }

    private async Task FinishGameAsync(GameToken gameToken, UserId endedBy)
    {
        var result = await _grains.GetGrain<IGameGrain>(gameToken).RequestGameEndAsync(endedBy);
        result.IsError.Should().BeFalse();
    }
}
