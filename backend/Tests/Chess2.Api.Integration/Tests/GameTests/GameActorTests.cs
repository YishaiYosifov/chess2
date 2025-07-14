using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.TestKit;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.GameTests;

public class GameActorTests : BaseAkkaIntegrationTest
{
    private const string TestGameToken = "testtoken";
    private readonly TimeControlSettings _timeControl = new(600, 5);
    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly ISanCalculator _sanCalculator;
    private readonly IMoveEncoder _moveEncoder;
    private readonly IGameCore _gameCore;
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
        _moveEncoder = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IMoveEncoder>();
        _gameCore = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameResultDescriber =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();

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
        var result = await _probe.ExpectMsgAsync<GameEvents.GameStateEvent>(
            cancellationToken: ApiTestBase.CT
        );

        var expectedClock = new ClockDto(
            WhiteClock: _timeControl.BaseSeconds * 1000,
            BlackClock: _timeControl.BaseSeconds * 1000,
            LastUpdated: _fakeNow.ToUnixTimeMilliseconds()
        );
        var expectedGameState = new GameState(
            WhitePlayer: _whitePlayer,
            BlackPlayer: _blackPlayer,
            SideToMove: GameColor.White,
            Clocks: expectedClock,
            Fen: _gameCore.Fen,
            MoveHistory: [],
            LegalMoves: _gameCore.GetLegalMovesFor(GameColor.White).EncodedMoves,
            TimeControl: _timeControl,
            IsRated: true
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
                new AlgebraicPoint("a2"),
                new AlgebraicPoint("c4")
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

        var move = GetLegalMoveFor(_whitePlayer);
        var expectedTimeLeft =
            _timeControl.BaseSeconds * 1000
            + _timeControl.IncrementSeconds * 1000 // add increment
            - 2 * 1000; // removed elapsed time

        MoveSnapshot expectedMoveSnapshot = new(
            EncodedMove: _moveEncoder.EncodeSingleMove(move),
            San: _sanCalculator.CalculateSan(
                move,
                _gameCore.GetLegalMovesFor(GameColor.White).AllMoves
            ),
            TimeLeft: expectedTimeLeft
        );
        ClockDto expectedClock = new(
            WhiteClock: expectedTimeLeft,
            BlackClock: _timeControl.BaseSeconds * 1000,
            LastUpdated: _fakeNow.ToUnixTimeMilliseconds()
        );

        await MakeLegalMoveAsync(_whitePlayer, move);

        await _gameNotifierMock
            .Received(1)
            .NotifyMoveMadeAsync(
                gameToken: TestGameToken,
                move: expectedMoveSnapshot,
                sideToMove: GameColor.Black,
                moveNumber: 1,
                clocks: ArgEx.FluentAssert<ClockDto>(x => x.Should().BeEquivalentTo(expectedClock)),
                sideToMoveUserId: _blackPlayer.UserId,
                _gameCore.GetLegalMovesFor(GameColor.Black).EncodedMoves
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
                new AlgebraicPoint("e2"),
                new AlgebraicPoint("e8")
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

        await _probe.ExpectMsgAsync<GameEvents.GameEnded>(cancellationToken: ApiTestBase.CT);

        var expectedEndStatus = _gameResultDescriber.Aborted(GameColor.White);
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
    }

    [Fact]
    public async Task EndGame_valid_user_should_return_win_for_opponent_after_early_moves()
    {
        await StartGameAsync();

        // Make enough moves to exceed abort threshold
        await MakeLegalMoveAsync(_whitePlayer);
        await MakeLegalMoveAsync(_blackPlayer);
        await MakeLegalMoveAsync(_whitePlayer);

        // Now White resigns — Black should win
        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);

        await _probe.ExpectMsgAsync<GameEvents.GameEnded>(cancellationToken: ApiTestBase.CT);

        var expectedEndStatus = _gameResultDescriber.Resignation(GameColor.White);
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
    }

    [Fact]
    public async Task EndGame_should_passivate_actor()
    {
        await StartGameAsync();

        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);
        await _probe.ExpectMsgAsync<GameEvents.GameEnded>(cancellationToken: ApiTestBase.CT);

        var passivate = await _parentProbe.ExpectMsgAsync<Passivate>(
            cancellationToken: ApiTestBase.CT
        );
        passivate.StopMessage.Should().Be(PoisonPill.Instance);
    }

    private Move GetLegalMoveFor(GamePlayer player) =>
        _gameCore.GetLegalMovesFor(player.Color).Moves.First().Value;

    private async Task<Move> MakeLegalMoveAsync(GamePlayer player, Move? move = null)
    {
        move ??= GetLegalMoveFor(player);
        _gameActor.Tell(
            new GameCommands.MovePiece(TestGameToken, player.UserId, move.From, move.To),
            _probe
        );

        await _probe.ExpectMsgAsync<GameEvents.PieceMoved>(
            cancellationToken: ApiTestBase.CT,
            duration: TimeSpan.FromSeconds(10)
        );

        return move;
    }

    [Fact]
    public async Task TickClock_should_end_game_when_time_runs_out()
    {
        await StartGameAsync(timeControl: new(0, 0));

        _gameActor.Tell(new GameCommands.TickClock(), _probe);
        await _probe.ExpectMsgAsync<GameEvents.GameEnded>(cancellationToken: ApiTestBase.CT);

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
        await _probe.ExpectMsgAsync<GameEvents.GameEnded>(cancellationToken: ApiTestBase.CT);

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
        await _probe.ExpectMsgAsync<GameEvents.GameStartedEvent>(
            cancellationToken: TestContext.Current.CancellationToken
        );
    }
}
