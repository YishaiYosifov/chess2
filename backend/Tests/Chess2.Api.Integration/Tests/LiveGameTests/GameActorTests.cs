using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.TestKit;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.LiveGameTests;

public class GameActorTests : BaseAkkaIntegrationTest
{
    private const string TestGameToken = "testtoken";
    private readonly TimeControlSettings _timeControl = new(600, 5);
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly ISanCalculator _sanCalculator;
    private readonly IGameCore _gameCore;
    private readonly IDrawRequestHandler _drawRequestHandler;
    private readonly IActorRef _gameActor;
    private readonly TestProbe _probe;
    private readonly TestProbe _parentProbe;

    private readonly IGameNotifier _gameNotifierMock = Substitute.For<IGameNotifier>();
    private readonly ITimerScheduler _timerMock = Substitute.For<ITimerScheduler>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IStopwatchProvider _stopwatchMock = Substitute.For<IStopwatchProvider>();

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    public GameActorTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _sanCalculator = ApiTestBase.Scope.ServiceProvider.GetRequiredService<ISanCalculator>();
        _gameCore = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameResultDescriber =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();
        _drawRequestHandler =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IDrawRequestHandler>();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        var clock = new GameClock(_timeProviderMock, _stopwatchMock);

        _parentProbe = CreateTestProbe();
        _gameActor = _parentProbe.ChildActorOf(
            Props.Create(
                () =>
                    new GameActor(
                        TestGameToken,
                        ApiTestBase.Scope.ServiceProvider,
                        _gameCore,
                        clock,
                        _gameResultDescriber,
                        _gameNotifierMock,
                        _drawRequestHandler,
                        _timerMock
                    )
            )
        );
        _probe = CreateTestProbe();
    }

    [Fact]
    public async Task IsGameOngoing_before_game_started_should_return_false_and_passivate()
    {
        _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);

        var result = await _probe.ExpectMsgAsync<bool>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.Should().BeFalse();

        var passivate = await _parentProbe.ExpectMsgAsync<Passivate>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        passivate.StopMessage.Should().Be(PoisonPill.Instance);
    }

    [Fact]
    public async Task StartGame_should_initialize_game_and_transition_to_playing_state()
    {
        _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);
        var isOngoing = await _probe.ExpectMsgAsync<bool>(cancellationToken: ApiTestBase.CT);
        isOngoing.Should().BeFalse();

        await StartGameAsync();

        _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);
        isOngoing = await _probe.ExpectMsgAsync<bool>(cancellationToken: ApiTestBase.CT);
        isOngoing.Should().BeTrue();

        _timerMock
            .Received(1)
            .StartPeriodicTimer(
                GameActor.ClockTimerKey,
                new GameCommands.TickClock(),
                TimeSpan.FromSeconds(1)
            );
    }

    [Fact]
    public async Task GetGameState_invalid_user_should_return_player_invalid_error()
    {
        await StartGameAsync();

        _gameActor.Tell(
            new GameQueries.GetGameState(TestGameToken, ForUserId: "invalid-user"),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task GetGameState_valid_user_should_return_GameStateEvent()
    {
        await StartGameAsync(isRated: true);

        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
        var result = await _probe.ExpectMsgAsync<GameResponses.GameStateResponse>(
            cancellationToken: ApiTestBase.CT
        );

        var expectedClock = new ClockSnapshot(
            WhiteClock: _timeControl.BaseSeconds * 1000,
            BlackClock: _timeControl.BaseSeconds * 1000,
            LastUpdated: _fakeNow.ToUnixTimeMilliseconds()
        );
        var legalMoves = _gameCore.GetLegalMovesFor(GameColor.White);
        var expectedGameState = new GameState(
            TimeControl: _timeControl,
            IsRated: true,
            WhitePlayer: _whitePlayer,
            BlackPlayer: _blackPlayer,
            Clocks: expectedClock,
            SideToMove: GameColor.White,
            InitialFen: _gameCore.InitialFen,
            MoveHistory: [],
            DrawState: _drawRequestHandler.GetDrawState(),
            MoveOptions: new(
                LegalMoves: legalMoves.MovePaths,
                HasForcedMoves: legalMoves.HasForcedMoves
            )
        );
        result.State.Should().BeEquivalentTo(expectedGameState);
    }

    [Fact]
    public async Task MovePiece_wrong_player_should_return_PlayerInvalid_error()
    {
        await StartGameAsync();

        _gameActor.Tell(
            new GameCommands.MovePiece(
                TestGameToken,
                _blackPlayer.UserId,
                Key: new(From: new AlgebraicPoint("a2"), To: new AlgebraicPoint("c4"))
            ),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task MovePiece_valid_should_send_PieceMoved()
    {
        await StartGameAsync();
        _stopwatchMock.Elapsed.Returns(TimeSpan.FromSeconds(2));

        var move = await MakeLegalMoveAsync(_whitePlayer);

        var expectedTimeLeft =
            _timeControl.BaseSeconds * 1000
            + _timeControl.IncrementSeconds * 1000 // add increment
            - 2 * 1000; // removed elapsed time

        MoveSnapshot expectedMoveSnapshot = new(
            Path: MovePath.FromMove(move, GameConstants.BoardWidth),
            San: _sanCalculator.CalculateSan(
                move,
                _gameCore.GetLegalMovesFor(GameColor.White).AllMoves
            ),
            TimeLeft: expectedTimeLeft
        );
        ClockSnapshot expectedClock = new(
            WhiteClock: expectedTimeLeft,
            BlackClock: _timeControl.BaseSeconds * 1000,
            LastUpdated: _fakeNow.ToUnixTimeMilliseconds()
        );
        var legalMoves = _gameCore.GetLegalMovesFor(GameColor.Black);
        await _gameNotifierMock
            .Received(1)
            .NotifyMoveMadeAsync(
                gameToken: TestGameToken,
                move: expectedMoveSnapshot,
                moveNumber: 1,
                clocks: ArgEx.FluentAssert<ClockSnapshot>(x =>
                    x.Should().BeEquivalentTo(expectedClock)
                ),
                sideToMove: GameColor.Black,
                sideToMoveUserId: _blackPlayer.UserId,
                encodedLegalMoves: legalMoves.EncodedMoves,
                hasForcedMoves: legalMoves.HasForcedMoves
            );
    }

    [Fact]
    public async Task MovePiece_invalid_should_return_error()
    {
        await StartGameAsync();

        _gameActor.Tell(
            new GameCommands.MovePiece(
                TestGameToken,
                _whitePlayer.UserId,
                Key: new(From: new AlgebraicPoint("e2"), To: new AlgebraicPoint("e8"))
            ),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public async Task MovePiece_that_results_in_draw_ends_the_game()
    {
        await StartGameAsync();

        var whiteMove1 = (from: new AlgebraicPoint("b1"), to: new AlgebraicPoint("c3"));
        var blackMove1 = (from: new AlgebraicPoint("b10"), to: new AlgebraicPoint("c8"));

        var whiteMove2 = (from: new AlgebraicPoint("c3"), to: new AlgebraicPoint("b1"));
        var blackMove2 = (from: new AlgebraicPoint("c8"), to: new AlgebraicPoint("b10"));

        for (int i = 0; i < 4; i++)
        {
            var (whiteFrom, whiteTo) = i % 2 == 0 ? whiteMove1 : whiteMove2;
            var (blackFrom, blackTo) = i % 2 == 0 ? blackMove1 : blackMove2;

            await MakeLegalMoveAsync(_whitePlayer, whiteFrom, whiteTo);
            await MakeLegalMoveAsync(_blackPlayer, blackFrom, blackTo);
        }

        await TestGameEndedAsync(_gameResultDescriber.ThreeFold());
    }

    [Fact]
    public async Task MovePiece_handles_forced_moves()
    {
        await StartGameAsync();

        // move the f pawn to e6, and then move the e pawn to e6
        // which creates a position with en passant
        await MakeLegalMoveAsync(_whitePlayer, new("f2"), new("f5"));
        await MakeLegalMoveAsync(_blackPlayer, new("f9"), new("f8"));
        await MakeLegalMoveAsync(_whitePlayer, new("f5"), new("f6"));

        _gameNotifierMock.ClearReceivedCalls();
        await MakeLegalMoveAsync(_blackPlayer, new("e9"), new("e6"));

        await _gameNotifierMock
            .Received(1)
            .NotifyMoveMadeAsync(
                Arg.Any<string>(),
                Arg.Any<MoveSnapshot>(),
                Arg.Any<int>(),
                Arg.Any<ClockSnapshot>(),
                Arg.Any<GameColor>(),
                Arg.Any<string>(),
                Arg.Any<byte[]>(),
                hasForcedMoves: true
            );

        _gameActor.Tell(
            new GameQueries.GetGameState(TestGameToken, ForUserId: _whitePlayer.UserId),
            _probe
        );
        var stateEvent = await _probe.ExpectMsgAsync<GameResponses.GameStateResponse>(
            cancellationToken: ApiTestBase.CT
        );
        stateEvent.State.MoveOptions.HasForcedMoves.Should().BeTrue();
        stateEvent.State.MoveOptions.LegalMoves.Should().HaveCount(1);
    }

    [Fact]
    public async Task MovePiece_decrements_draw_cooldown()
    {
        await StartGameAsync();
        _gameActor.Tell(new GameCommands.RequestDraw(TestGameToken, _whitePlayer.UserId), _probe);
        await _probe.ExpectMsgAsync<GameResponses.DrawRequested>(cancellationToken: ApiTestBase.CT);
        _gameActor.Tell(new GameCommands.DeclineDraw(TestGameToken, _whitePlayer.UserId), _probe);
        await _probe.ExpectMsgAsync<GameResponses.DrawDeclined>(cancellationToken: ApiTestBase.CT);
        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
        var initialCooldown = (
            await _probe.ExpectMsgAsync<GameResponses.GameStateResponse>(
                cancellationToken: ApiTestBase.CT
            )
        )
            .State
            .DrawState
            .Cooldown[GameColor.White];
        _gameNotifierMock.ClearReceivedCalls();

        await MakeLegalMoveAsync(_whitePlayer);

        await _gameNotifierMock.DidNotReceive().NotifyDrawDeclinedAsync(Arg.Any<string>());
        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
        var state = await _probe.ExpectMsgAsync<GameResponses.GameStateResponse>(
            cancellationToken: ApiTestBase.CT
        );

        state.State.DrawState.Cooldown[GameColor.White].Should().Be(initialCooldown - 1);
        state.State.DrawState.Cooldown.GetValueOrDefault(GameColor.Black).Should().Be(0);
    }

    [Fact]
    public async Task MovePiece_declines_pending_draw_request()
    {
        await StartGameAsync();
        _gameActor.Tell(new GameCommands.RequestDraw(TestGameToken, _whitePlayer.UserId), _probe);
        await _probe.ExpectMsgAsync<GameResponses.DrawRequested>(cancellationToken: ApiTestBase.CT);

        await MakeLegalMoveAsync(_whitePlayer);

        await _gameNotifierMock.Received(1).NotifyDrawDeclinedAsync(TestGameToken);

        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
        var state = await _probe.ExpectMsgAsync<GameResponses.GameStateResponse>(
            cancellationToken: ApiTestBase.CT
        );
        state.State.DrawState.ActiveRequester.Should().BeNull();
    }

    [Fact]
    public async Task EndGame_invalid_user_should_return_PlayerInvalid()
    {
        await StartGameAsync();

        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, "nonexistent-user"), _probe);

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task EndGame_valid_user_should_return_Aborted_if_early_game()
    {
        await StartGameAsync();

        // No moves or just one move = still abortable
        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);

        await _probe.ExpectMsgAsync<GameResponses.GameEnded>(cancellationToken: ApiTestBase.CT);
        await TestGameEndedAsync(_gameResultDescriber.Aborted(GameColor.White));
    }

    [Fact]
    public async Task EndGame_valid_user_should_return_win_for_opponent_after_early_moves()
    {
        await StartGameAsync();

        // make enough moves to exceed abort threshold
        await MakeLegalMoveAsync(_whitePlayer);
        await MakeLegalMoveAsync(_blackPlayer);
        await MakeLegalMoveAsync(_whitePlayer);

        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);
        await _probe.ExpectMsgAsync<GameResponses.GameEnded>(cancellationToken: ApiTestBase.CT);
        await TestGameEndedAsync(_gameResultDescriber.Resignation(GameColor.White));
    }

    [Fact]
    public async Task TickClock_should_end_game_when_time_runs_out()
    {
        await StartGameAsync(timeControl: new(0, 0));

        _gameActor.Tell(new GameCommands.TickClock(), _probe);
        await _probe.ExpectMsgAsync<GameResponses.GameEnded>(cancellationToken: ApiTestBase.CT);

        var expectedEndStatus = _gameResultDescriber.Timeout(GameColor.White);
        await _gameNotifierMock
            .Received(1)
            .NotifyGameEndedAsync(
                TestGameToken,
                ArgEx.FluentAssert<GameResultData>(
                    (x) =>
                    {
                        x.Result.Should().Be(expectedEndStatus.Result);
                        x.ResultDescription.Should().Be(expectedEndStatus.ResultDescription);
                    }
                )
            );

        var passivate = await _parentProbe.ExpectMsgAsync<Passivate>(
            cancellationToken: ApiTestBase.CT
        );
        passivate.StopMessage.Should().Be(PoisonPill.Instance);
        _timerMock.Received(1).Cancel(GameActor.ClockTimerKey);
    }

    [Fact]
    public async Task After_game_finishes_actor_becomes_finished()
    {
        await StartGameAsync();

        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);
        await _probe.ExpectMsgAsync<GameResponses.GameEnded>(cancellationToken: ApiTestBase.CT);

        _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);
        var isGameOngoing = await _probe.ExpectMsgAsync<bool>(cancellationToken: ApiTestBase.CT);
        isGameOngoing.Should().BeFalse();

        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
        var stateResult = await _probe.ExpectMsgAsync<ErrorOr<object>>(
            cancellationToken: ApiTestBase.CT
        );
        stateResult.IsError.Should().BeTrue();
        stateResult.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.GameAlreadyEnded);
    }

    private Move GetLegalMoveFor(GamePlayer player) =>
        _gameCore.GetLegalMovesFor(player.Color).MovesMap.First().Value;

    private async Task<Move> MakeLegalMoveAsync(GamePlayer player)
    {
        var move = GetLegalMoveFor(player);
        await MakeLegalMoveAsync(player, move.From, move.To);
        return move;
    }

    private async Task MakeLegalMoveAsync(GamePlayer player, AlgebraicPoint from, AlgebraicPoint to)
    {
        _gameActor.Tell(
            new GameCommands.MovePiece(TestGameToken, player.UserId, Key: new(from, to)),
            _probe
        );

        await _probe.ExpectMsgAsync<GameResponses.PieceMoved>(
            cancellationToken: ApiTestBase.CT,
            duration: TimeSpan.FromHours(10)
        );
    }

    private async Task StartGameAsync(
        GamePlayer? whitePlayer = null,
        GamePlayer? blackPlayer = null,
        TimeControlSettings? timeControl = null,
        bool isRated = true
    )
    {
        _gameActor.Tell(
            new GameCommands.StartGame(
                TestGameToken,
                WhitePlayer: whitePlayer ?? _whitePlayer,
                BlackPlayer: blackPlayer ?? _blackPlayer,
                TimeControl: timeControl ?? _timeControl,
                isRated
            ),
            _probe.Ref
        );
        await _probe.ExpectMsgAsync<GameResponses.GameStarted>(
            cancellationToken: TestContext.Current.CancellationToken
        );
    }

    private async Task TestGameEndedAsync(GameEndStatus expectedEndStatus)
    {
        await _gameNotifierMock
            .Received(1)
            .NotifyGameEndedAsync(
                TestGameToken,
                ArgEx.FluentAssert<GameResultData>(
                    (x) =>
                    {
                        x.Result.Should().Be(expectedEndStatus.Result);
                        x.ResultDescription.Should().Be(expectedEndStatus.ResultDescription);
                    }
                )
            );

        var passivate = await _parentProbe.ExpectMsgAsync<Passivate>(
            cancellationToken: ApiTestBase.CT
        );
        passivate.StopMessage.Should().Be(PoisonPill.Instance);
    }
}
