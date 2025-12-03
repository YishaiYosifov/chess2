using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.Game.SignalR;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameNotifierTests
{
    private readonly GameToken _gameToken = "game-123";
    private readonly ConnectionId _connId = "conn1";
    private readonly UserId _userId = "user1";

    private readonly IHubContext<GameHub, IGameHubClient> _hubContextMock = Substitute.For<
        IHubContext<GameHub, IGameHubClient>
    >();
    private readonly IHubClients<IGameHubClient> _clientsMock = Substitute.For<
        IHubClients<IGameHubClient>
    >();
    private readonly IGroupManager _groupsMock = Substitute.For<IGroupManager>();

    private readonly IGameHubClient _clientGameGroupProxyMock = Substitute.For<IGameHubClient>();
    private readonly IGameHubClient _clientUserGameGroupProxyMock =
        Substitute.For<IGameHubClient>();
    private readonly IGameHubClient _clientConnProxyMock = Substitute.For<IGameHubClient>();

    private readonly GameNotifier _notifier;

    public GameNotifierTests()
    {
        _clientsMock.Group(_gameToken).Returns(_clientGameGroupProxyMock);
        _clientsMock.Group($"{_gameToken}:{_userId}").Returns(_clientUserGameGroupProxyMock);
        _clientsMock.Client(_connId).Returns(_clientConnProxyMock);

        _hubContextMock.Clients.Returns(_clientsMock);
        _hubContextMock.Groups.Returns(_groupsMock);

        _notifier = new(_hubContextMock);
    }

    [Fact]
    public async Task SyncRevisionAsync_sends_revision_to_specific_connection()
    {
        GameNotifierState state = new() { Revision = 42 };

        await _notifier.SyncRevisionAsync(_connId, state);

        await _clientConnProxyMock.Received(1).SyncRevisionAsync(42);
    }

    [Fact]
    public async Task NotifyMoveMadeAsync_sends_move_and_legal_moves_and_increments_revision()
    {
        GameNotifierState state = new() { Revision = 0 };
        MoveNotification notification = new(
            GameToken: _gameToken,
            Move: new MoveSnapshotFaker().Generate(),
            MoveNumber: 5,
            Clocks: new ClockSnapshot(
                WhiteClock: 10,
                BlackClock: 20,
                LastUpdated: 1000,
                IsFrozen: false
            ),
            SideToMove: GameColor.White,
            SideToMoveUserId: _userId,
            LegalMoves: [1, 2, 3],
            HasForcedMoves: true
        );

        await _notifier.NotifyMoveMadeAsync(notification, state);

        await _clientGameGroupProxyMock
            .Received(1)
            .MoveMadeAsync(
                notification.Move,
                notification.SideToMove,
                notification.MoveNumber,
                notification.Clocks
            );
        await _clientUserGameGroupProxyMock
            .Received(1)
            .LegalMovesChangedAsync(notification.LegalMoves, notification.HasForcedMoves);
        state.Revision.Should().Be(1);
    }

    [Fact]
    public async Task NotifyDrawStateChangeAsync_sends_draw_state_and_increments_revision()
    {
        GameNotifierState state = new() { Revision = 3 };
        DrawState drawState = new(ActiveRequester: GameColor.Black);

        await _notifier.NotifyDrawStateChangeAsync(_gameToken, drawState, state);

        await _clientGameGroupProxyMock.Received(1).DrawStateChangeAsync(drawState);
        state.Revision.Should().Be(4);
    }

    [Fact]
    public async Task NotifyGameEndedAsync_sends_result_and_increments_revision()
    {
        GameNotifierState state = new() { Revision = 5 };
        GameResultData result = new GameResultDataFaker().Generate();
        ClockSnapshot finalClocks = new(
            WhiteClock: 10,
            BlackClock: 20,
            LastUpdated: 1000,
            IsFrozen: true
        );

        await _notifier.NotifyGameEndedAsync(_gameToken, result, finalClocks, state);

        await _clientGameGroupProxyMock.Received(1).GameEndedAsync(result, finalClocks);
        state.Revision.Should().Be(6);
    }

    [Fact]
    public async Task JoinGameGroupAsync_adds_connection_to_game_and_user_groups()
    {
        await _notifier.JoinGameGroupAsync(_gameToken, _userId, _connId);

        await _groupsMock
            .Received(1)
            .AddToGroupAsync(_connId, _gameToken, Arg.Any<CancellationToken>());
        await _groupsMock
            .Received(1)
            .AddToGroupAsync(_connId, $"{_gameToken}:{_userId}", Arg.Any<CancellationToken>());
    }
}
