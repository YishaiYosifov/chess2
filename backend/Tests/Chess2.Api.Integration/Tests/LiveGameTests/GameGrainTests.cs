using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.LiveGameTests;

public class GameGrainTests : BaseOrleansIntegrationTest
{
    private const string TestGameToken = "testtoken";

    private readonly TimeControlSettings _timeControl = new(600, 5);

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly ISanCalculator _sanCalculator;
    private readonly IGameCore _gameCore;
    private readonly IDrawRequestHandler _drawRequestHandler;
    private readonly GameSettings _settings;

    //private readonly IActorRef _gameActor;
    //private readonly TestProbe _probe;
    //private readonly TestProbe _parentProbe;

    private readonly IGameNotifier _gameNotifierMock = Substitute.For<IGameNotifier>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IStopwatchProvider _stopwatchMock = Substitute.For<IStopwatchProvider>();

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    public GameGrainTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _sanCalculator = ApiTestBase.Scope.ServiceProvider.GetRequiredService<ISanCalculator>();
        _gameCore = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameResultDescriber =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();
        _drawRequestHandler =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IDrawRequestHandler>();
        _settings = ApiTestBase
            .Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>()
            .Value.Game;
        var gameFinalizer = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameFinalizer>();
        var clock = new GameClock(_timeProviderMock, _stopwatchMock);

        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        Silo.ServiceProvider.AddService(_gameCore);
        Silo.ServiceProvider.AddService<IGameClock>(clock);
        Silo.ServiceProvider.AddService(_gameResultDescriber);
        Silo.ServiceProvider.AddService(_gameNotifierMock);
        Silo.ServiceProvider.AddService(_drawRequestHandler);
        Silo.ServiceProvider.AddService(gameFinalizer);
    }

    private async Task<IGameGrain> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<GameGrain>(TestGameToken);

    //[Fact]
    //public async Task IsGameOngoing_before_game_started_should_return_false_and_passivate()
    //{
    //    _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);

    //    var result = await _probe.ExpectMsgAsync<bool>(
    //        cancellationToken: TestContext.Current.CancellationToken
    //    );
    //    result.Should().BeFalse();

    //    var passivate = await _parentProbe.ExpectMsgAsync<Passivate>(
    //        cancellationToken: TestContext.Current.CancellationToken
    //    );
    //    passivate.StopMessage.Should().Be(PoisonPill.Instance);
    //}

    //[Fact]
    //public async Task StartGame_should_initialize_game_and_transition_to_playing_state()
    //{
    //    _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);
    //    var isOngoing = await _probe.ExpectMsgAsync<bool>(cancellationToken: ApiTestBase.CT);
    //    isOngoing.Should().BeFalse();

    //    await StartGameAsync();

    //    _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);
    //    isOngoing = await _probe.ExpectMsgAsync<bool>(cancellationToken: ApiTestBase.CT);
    //    isOngoing.Should().BeTrue();

    //    _timerMock
    //        .Received(1)
    //        .StartPeriodicTimer(
    //            GameGrain.ClockTimerKey,
    //            new GameCommands.TickClock(),
    //            TimeSpan.FromSeconds(1)
    //        );
    //}

    [Fact]
    public async Task GetGameStateAsync_for_an_invalid_user_returns_PlayerInvalid_error()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.GetStateAsync("invalid-user");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task GetGameState_for_a_valid_user_returns_correct_GameStateEvent()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain, isRated: true);

        var result = await grain.GetStateAsync(_whitePlayer.UserId);

        result.IsError.Should().BeFalse();
        var expectedClock = new ClockSnapshot(
            WhiteClock: _timeControl.BaseSeconds * 1000,
            BlackClock: _timeControl.BaseSeconds * 1000,
            LastUpdated: _fakeNow.ToUnixTimeMilliseconds()
        );
        var legalMoves = _gameCore.GetLegalMoves(GameColor.White);
        var expectedGameState = new GameState(
            TimeControl: _timeControl,
            IsRated: true,
            WhitePlayer: _whitePlayer,
            BlackPlayer: _blackPlayer,
            Clocks: expectedClock,
            SideToMove: GameColor.White,
            InitialFen: _gameCore.InitialFen,
            MoveHistory: [],
            DrawState: _drawRequestHandler.GetState(),
            MoveOptions: new(
                LegalMoves: legalMoves.MovePaths,
                HasForcedMoves: legalMoves.HasForcedMoves
            )
        );
        result.Value.Should().BeEquivalentTo(expectedGameState);
    }

    [Fact]
    public async Task RequestDraw_sends_notification_if_no_pending_request()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.RequestDrawAsync(_whitePlayer.UserId);
        result.IsError.Should().BeFalse();

        await _gameNotifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                TestGameToken,
                new DrawState(ActiveRequester: GameColor.White)
            );

        var state = await grain.GetStateAsync();
        state.Value.DrawState.ActiveRequester.Should().Be(GameColor.White);
    }

    [Fact]
    public async Task RequestDraw_ends_the_game_if_there_is_a_pending_request()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        await grain.RequestDrawAsync(_whitePlayer.UserId);
        await grain.RequestDrawAsync(_blackPlayer.UserId);

        await TestGameEndedAsync(grain, _gameResultDescriber.DrawByAgreement());
    }

    [Fact]
    public async Task DeclineDraw_declines_the_draw_correctly()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        await grain.RequestDrawAsync(_whitePlayer.UserId);
        await grain.DeclineDrawAsync(_blackPlayer.UserId);

        await _gameNotifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                TestGameToken,
                new DrawState(WhiteCooldown: _settings.DrawCooldown)
            );

        var state = await grain.GetStateAsync();
        state.Value.DrawState.ActiveRequester.Should().BeNull();
    }

    [Fact]
    public async Task MovePiece_with_the_wrong_player_should_return_PlayerInvalid_error()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.MovePieceAsync(
            _blackPlayer.UserId,
            key: new(From: new AlgebraicPoint("a2"), To: new AlgebraicPoint("c4"))
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task MovePiece_with_a_valid_move_creates_a_correct_move_made_notification()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);
        _stopwatchMock.Elapsed.Returns(TimeSpan.FromSeconds(2));

        var move = await MakeLegalMoveAsync(grain, _whitePlayer);

        var expectedTimeLeft =
            _timeControl.BaseSeconds * 1000
            + _timeControl.IncrementSeconds * 1000 // add increment
            - 2 * 1000; // removed elapsed time

        MoveSnapshot expectedMoveSnapshot = new(
            Path: MovePath.FromMove(move, GameConstants.BoardWidth),
            San: _sanCalculator.CalculateSan(
                move,
                _gameCore.GetLegalMoves(GameColor.White).AllMoves
            ),
            TimeLeft: expectedTimeLeft
        );
        ClockSnapshot expectedClock = new(
            WhiteClock: expectedTimeLeft,
            BlackClock: _timeControl.BaseSeconds * 1000,
            LastUpdated: _fakeNow.ToUnixTimeMilliseconds()
        );
        var legalMoves = _gameCore.GetLegalMoves(GameColor.Black);
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
    public async Task MovePiece_with_an_invalid_move_should_returns_an_error()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.MovePieceAsync(
            _whitePlayer.UserId,
            new(From: new AlgebraicPoint("e2"), To: new AlgebraicPoint("e8"))
        );
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public async Task MovePiece_that_results_in_draw_ends_the_game()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var whiteMove1 = (from: new AlgebraicPoint("b1"), to: new AlgebraicPoint("c3"));
        var blackMove1 = (from: new AlgebraicPoint("b10"), to: new AlgebraicPoint("c8"));

        var whiteMove2 = (from: new AlgebraicPoint("c3"), to: new AlgebraicPoint("b1"));
        var blackMove2 = (from: new AlgebraicPoint("c8"), to: new AlgebraicPoint("b10"));

        for (int i = 0; i < 4; i++)
        {
            var (whiteFrom, whiteTo) = i % 2 == 0 ? whiteMove1 : whiteMove2;
            var (blackFrom, blackTo) = i % 2 == 0 ? blackMove1 : blackMove2;

            await grain.MovePieceAsync(_whitePlayer.UserId, new(whiteFrom, whiteTo));
            await grain.MovePieceAsync(_blackPlayer.UserId, new(blackFrom, blackTo));
        }

        await TestGameEndedAsync(grain, _gameResultDescriber.ThreeFold());
    }

    [Fact]
    public async Task MovePiece_handles_forced_moves()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        // move the f pawn to e6, and then move the e pawn to e6
        // which creates a position with en passant
        await grain.MovePieceAsync(_whitePlayer.UserId, new(new("f2"), new("f5")));
        await grain.MovePieceAsync(_blackPlayer.UserId, new(new("f9"), new("f8")));
        await grain.MovePieceAsync(_whitePlayer.UserId, new(new("f5"), new("f6")));

        _gameNotifierMock.ClearReceivedCalls();
        await grain.MovePieceAsync(_blackPlayer.UserId, new(new("e9"), new("e6")));

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

        var state = await grain.GetStateAsync(_whitePlayer.UserId);
        state.Value.MoveOptions.HasForcedMoves.Should().BeTrue();
        state.Value.MoveOptions.LegalMoves.Should().HaveCount(1);
    }

    [Fact]
    public async Task MovePiece_decrements_draw_cooldown()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        await grain.RequestDrawAsync(_whitePlayer.UserId);
        await grain.DeclineDrawAsync(_blackPlayer.UserId);

        var initialState = await grain.GetStateAsync();
        var drawCooldown = initialState.Value.DrawState.WhiteCooldown;

        _gameNotifierMock.ClearReceivedCalls();

        await MakeLegalMoveAsync(grain, _whitePlayer);

        await _gameNotifierMock
            .DidNotReceive()
            .NotifyDrawStateChangeAsync(Arg.Any<string>(), Arg.Any<DrawState>());

        var state = await grain.GetStateAsync();
        state.Value.DrawState.WhiteCooldown.Should().Be(drawCooldown - 1);
        state.Value.DrawState.BlackCooldown.Should().Be(0);
    }

    [Fact]
    public async Task MovePiece_declines_pending_draw_request()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        await grain.RequestDrawAsync(_whitePlayer.UserId);

        await MakeLegalMoveAsync(grain, _whitePlayer);
        await MakeLegalMoveAsync(grain, _blackPlayer);

        await _gameNotifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                TestGameToken,
                new DrawState(WhiteCooldown: _settings.DrawCooldown)
            );

        var state = await grain.GetStateAsync();
        state.Value.DrawState.ActiveRequester.Should().BeNull();
    }

    [Fact]
    public async Task EndGame_returns_PlayerInvalid_error_for_invalid_players()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.EndGameAsync("nonexistent-user");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task EndGame_aborts_the_game_if_not_enough_moves_have_been_made()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        // No moves or just one move = still abortable
        await grain.EndGameAsync(_whitePlayer.UserId);

        await TestGameEndedAsync(grain, _gameResultDescriber.Aborted(GameColor.White));
    }

    [Fact]
    public async Task EndGame_valid_user_should_return_win_for_opponent_after_early_moves()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        // make enough moves to exceed abort threshold
        await MakeLegalMoveAsync(grain, _whitePlayer);
        await MakeLegalMoveAsync(grain, _blackPlayer);
        await MakeLegalMoveAsync(grain, _whitePlayer);

        await grain.EndGameAsync(_whitePlayer.UserId);
        await TestGameEndedAsync(grain, _gameResultDescriber.Resignation(GameColor.White));
    }

    //[Fact]
    //public async Task TickClock_should_end_game_when_time_runs_out()
    //{
    //    await StartGameAsync(timeControl: new(0, 0));

    //    _gameActor.Tell(new GameCommands.TickClock(), _probe);
    //    await _probe.ExpectMsgAsync<GameReplies.GameEnded>(cancellationToken: ApiTestBase.CT);

    //    var expectedEndStatus = _gameResultDescriber.Timeout(GameColor.White);
    //    await _gameNotifierMock
    //        .Received(1)
    //        .NotifyGameEndedAsync(
    //            TestGameToken,
    //            ArgEx.FluentAssert<GameResultData>(
    //                (x) =>
    //                {
    //                    x.Result.Should().Be(expectedEndStatus.Result);
    //                    x.ResultDescription.Should().Be(expectedEndStatus.ResultDescription);
    //                }
    //            )
    //        );

    //    var passivate = await _parentProbe.ExpectMsgAsync<Passivate>(
    //        cancellationToken: ApiTestBase.CT
    //    );
    //    passivate.StopMessage.Should().Be(PoisonPill.Instance);
    //    _timerMock.Received(1).Cancel(GameGrain.ClockTimerKey);
    //}

    //[Fact]
    //public async Task After_game_finishes_actor_becomes_finished()
    //{
    //    await StartGameAsync();

    //    _gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);
    //    await _probe.ExpectMsgAsync<GameReplies.GameEnded>(cancellationToken: ApiTestBase.CT);

    //    _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);
    //    var isGameOngoing = await _probe.ExpectMsgAsync<bool>(cancellationToken: ApiTestBase.CT);
    //    isGameOngoing.Should().BeFalse();

    //    _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
    //    var stateResult = await _probe.ExpectMsgAsync<ErrorOr<object>>(
    //        cancellationToken: ApiTestBase.CT
    //    );
    //    stateResult.IsError.Should().BeTrue();
    //    stateResult.FirstError.Should().Be(GameErrors.GameAlreadyEnded);
    //}

    private Move GetLegalMoveFor(GamePlayer player) =>
        _gameCore.GetLegalMoves(player.Color).MovesMap.First().Value;

    private async Task<Move> MakeLegalMoveAsync(IGameGrain grain, GamePlayer player)
    {
        var move = GetLegalMoveFor(player);
        await grain.MovePieceAsync(player.UserId, key: new(move.From, move.To));
        return move;
    }

    private Task StartGameAsync(
        IGameGrain grain,
        GamePlayer? whitePlayer = null,
        GamePlayer? blackPlayer = null,
        TimeControlSettings? timeControl = null,
        bool isRated = true
    ) =>
        grain.StartGameAsync(
            whitePlayer: whitePlayer ?? _whitePlayer,
            blackPlayer: blackPlayer ?? _blackPlayer,
            timeControl: timeControl ?? _timeControl,
            isRated: isRated
        );

    private async Task TestGameEndedAsync(IGameGrain grain, GameEndStatus expectedEndStatus)
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
        var isOngoing = await grain.IsGameOngoingAsync();
        isOngoing.Should().BeFalse();

        //var passivate = await _parentProbe.ExpectMsgAsync<Passivate>(
        //    cancellationToken: ApiTestBase.CT
        //);
        //passivate.StopMessage.Should().Be(PoisonPill.Instance);
    }
}
