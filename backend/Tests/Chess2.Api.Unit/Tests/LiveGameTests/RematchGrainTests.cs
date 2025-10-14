using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Grains;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class RematchGrainTests : BaseGrainTest
{
    private readonly IRematchNotifier _rematchNotifierMock = Substitute.For<IRematchNotifier>();
    private readonly IGameGrain _gameGrainMock = Substitute.For<IGameGrain>();
    private readonly IGameStarter _gameStarterMock = Substitute.For<IGameStarter>();

    private readonly GameSettings _settings;

    private readonly GameState _gameState;
    private readonly GameToken _gameToken = "test token";

    private readonly RematchGrainState _state;
    private readonly TestStorageStats _stateStats;

    public RematchGrainTests()
    {
        var settings = AppSettingsLoader.LoadAppSettings();
        _settings = settings.Game;

        Silo.AddProbe(id =>
        {
            if (id.ToString() == _gameToken)
                return _gameGrainMock;
            return Substitute.For<IGameGrain>();
        });

        Silo.AddService(Options.Create(settings));
        Silo.AddService(_rematchNotifierMock);
        Silo.AddService(_gameStarterMock);

        _state = Silo.StorageManager.GetStorage<RematchGrainState>(RematchGrain.StateName).State;
        _stateStats = Silo.StorageManager.GetStorageStats(RematchGrain.StateName)!;

        _gameState = new GameStateFaker()
            .RuleFor(x => x.ResultData, new GameResultDataFaker().Generate())
            .Generate();
        _gameGrainMock.GetStateAsync(forUserId: null).Returns(_gameState);
    }

    private Task<RematchGrain> CreateGrainAsync() =>
        Silo.CreateGrainAsync<RematchGrain>(_gameToken);

    [Fact]
    public async Task RequestAsync_rejects_when_the_game_is_not_found()
    {
        _gameGrainMock.GetStateAsync(forUserId: null).Returns(GameErrors.GameNotFound);
        var grain = await CreateGrainAsync();

        var result = await grain.RequestAsync(_gameState.WhitePlayer.UserId, "test conn");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.GameNotFound);
        _state.Request.Should().BeNull();
    }

    [Fact]
    public async Task RequestAsync_rejects_when_the_game_is_not_over()
    {
        _gameGrainMock
            .GetStateAsync(forUserId: null)
            .Returns(_gameState with { ResultData = null });
        var grain = await CreateGrainAsync();

        var result = await grain.RequestAsync(_gameState.WhitePlayer.UserId, "test conn");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.GameNotOver);
        _state.Request.Should().BeNull();
    }

    [Fact]
    public async Task RequestAsync_sets_up_rematch_correctly()
    {
        var grain = await CreateGrainAsync();
        var player = _gameState.WhitePlayer;
        var connection = "white-conn";

        var result = await grain.RequestAsync(player.UserId, connection);

        result.IsError.Should().BeFalse();
        _state.WhiteConnections.Should().Contain(connection);
        _state.BlackConnections.Should().BeEmpty();
        await _rematchNotifierMock
            .Received(1)
            .NotifyRematchRequestedAsync(_gameState.BlackPlayer.UserId);

        Silo.ReminderRegistry.Mock.Verify(x =>
            x.RegisterOrUpdateReminder(
                Silo.GetGrainId(grain),
                RematchGrain.ExpirationReminderName,
                _settings.RematchLifetime,
                _settings.RematchLifetime
            )
        );

        _state.Request.Should().NotBeNull();
        _state.Request!.Players.WhitePlayer.Should().BeEquivalentTo(_gameState.WhitePlayer);
        _state.Request.Players.BlackPlayer.Should().BeEquivalentTo(_gameState.BlackPlayer);
    }

    [Fact]
    public async Task RequestAsync_accepts_rematch_when_both_players_connected()
    {
        var createdGameToken = "created game token";
        _gameStarterMock
            .StartGameAsync(
                _gameState.WhitePlayer.UserId,
                _gameState.BlackPlayer.UserId,
                _gameState.Pool
            )
            .Returns(createdGameToken);

        var grain = await CreateGrainAsync();

        await grain.RequestAsync(_gameState.WhitePlayer.UserId, "white conn");
        var result = await grain.RequestAsync(_gameState.BlackPlayer.UserId, "black conn");

        result.IsError.Should().BeFalse();
        _stateStats.Clears.Should().Be(1);
        await _rematchNotifierMock
            .Received(1)
            .NotifyRematchAccepted(
                createdGameToken,
                _gameState.WhitePlayer.UserId,
                _gameState.BlackPlayer.UserId
            );
    }

    [Fact]
    public async Task RequestAsync_rejects_invalid_player()
    {
        var grain = await CreateGrainAsync();
        var invalidPlayer = "invalid-player-id";

        var result = await grain.RequestAsync(invalidPlayer, "conn");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task CancelAsync_clears_state_and_notifies()
    {
        var grain = await CreateGrainAsync();
        await grain.RequestAsync(_gameState.WhitePlayer.UserId, "white-conn");

        var result = await grain.CancelAsync(_gameState.WhitePlayer.UserId);

        result.IsError.Should().BeFalse();
        _stateStats.Clears.Should().Be(1);
        await _rematchNotifierMock
            .Received(1)
            .NotifyRematchCancelledAsync(
                _gameState.WhitePlayer.UserId,
                _gameState.BlackPlayer.UserId
            );
    }

    [Fact]
    public async Task CancelAsync_rejects_invalid_player()
    {
        var grain = await CreateGrainAsync();
        await grain.RequestAsync(_gameState.WhitePlayer.UserId, "white-conn");

        var invalidPlayer = "invalid-player-id";

        var result = await grain.CancelAsync(invalidPlayer);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task RemoveConnectionAsync_removes_connection_and_tears_down_if_empty()
    {
        var grain = await CreateGrainAsync();

        var player = _gameState.WhitePlayer;
        var connection = "white-conn";

        await grain.RequestAsync(player.UserId, connection);

        var result = await grain.RemoveConnectionAsync(player.UserId, connection);

        result.IsError.Should().BeFalse();
        _state.WhiteConnections.Should().BeEmpty();
        _stateStats.Clears.Should().Be(1);
        await _rematchNotifierMock
            .Received(1)
            .NotifyRematchCancelledAsync(player.UserId, _gameState.BlackPlayer.UserId);
    }

    [Fact]
    public async Task RemoveConnectionAsync_removes_connection_without_teardown_if_others_present()
    {
        var grain = await CreateGrainAsync();

        var player = _gameState.WhitePlayer;

        await grain.RequestAsync(player.UserId, "conn1");
        await grain.RequestAsync(player.UserId, "conn2");

        var result = await grain.RemoveConnectionAsync(player.UserId, "conn1");

        result.IsError.Should().BeFalse();
        _state.WhiteConnections.Should().ContainSingle().Which.Should().Be((ConnectionId)"conn2");
        _stateStats.Clears.Should().Be(0);
    }

    [Fact]
    public async Task RemoveConnectionAsync_rejects_invalid_player()
    {
        var grain = await CreateGrainAsync();
        await grain.RequestAsync(_gameState.WhitePlayer.UserId, "white-conn");
        var invalidPlayer = "invalid-player-id";

        var result = await grain.RemoveConnectionAsync(invalidPlayer, "conn");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task ReceiveReminder_cancels_rematch()
    {
        var grain = await CreateGrainAsync();

        await grain.RequestAsync(_gameState.WhitePlayer.UserId, "white-conn");

        await Silo.FireAllReminders();

        _stateStats.Clears.Should().Be(1);
        await _rematchNotifierMock
            .Received(1)
            .NotifyRematchCancelledAsync(
                _gameState.WhitePlayer.UserId,
                _gameState.BlackPlayer.UserId
            );
    }
}
