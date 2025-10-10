using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Grains;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;

namespace Chess2.Api.Integration.Tests.LiveGameTests;

public class GameGrainTests : BaseOrleansIntegrationTest
{
    private readonly GameToken _gameToken = "testtoken";
    private readonly PoolKey _pool = new(
        PoolType.Rated,
        new(BaseSeconds: 600, IncrementSeconds: 5)
    );

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly ISanCalculator _sanCalculator;
    private readonly IGameCore _gameCore;
    private readonly GameSettings _settings;

    private readonly IGameNotifier _gameNotifierMock = Substitute.For<IGameNotifier>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    private readonly GameGrainState _state;
    private readonly TestStorageStats _stateStats;

    public GameGrainTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _sanCalculator = ApiTestBase.Scope.ServiceProvider.GetRequiredService<ISanCalculator>();
        _gameCore = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameResultDescriber =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();

        var settings = ApiTestBase.Scope.ServiceProvider.GetRequiredService<
            IOptions<AppSettings>
        >();
        var gameFinalizer = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameFinalizer>();
        GameClock clock = new(_timeProviderMock);

        _settings = settings.Value.Game;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        Silo.ServiceProvider.AddService(_gameCore);
        Silo.ServiceProvider.AddService<IGameClock>(clock);
        Silo.ServiceProvider.AddService(_gameResultDescriber);
        Silo.ServiceProvider.AddService(_gameNotifierMock);
        Silo.ServiceProvider.AddService(gameFinalizer);
        Silo.ServiceProvider.AddService(settings);

        _state = Silo.StorageManager.GetStorage<GameGrainState>(GameGrain.StateName).State;
        _stateStats = Silo.StorageManager.GetStorageStats(GameGrain.StateName)!;
    }

    private async Task<IGameGrain> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<GameGrain>(_gameToken);

    [Fact]
    public async Task GetStateAsync_returns_the_correct_GameStateEvent()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.GetStateAsync(_whitePlayer.UserId);

        result.IsError.Should().BeFalse();
        ClockSnapshot expectedClock = new(
            WhiteClock: _pool.TimeControl.BaseSeconds * 1000,
            BlackClock: _pool.TimeControl.BaseSeconds * 1000,
            LastUpdated: _fakeNow.ToUnixTimeMilliseconds()
        );
        var legalMoves = _gameCore.GetLegalMovesOf(GameColor.White, _state.Core);
        GameState expectedGameState = new(
            Pool: _pool,
            WhitePlayer: _whitePlayer,
            BlackPlayer: _blackPlayer,
            Clocks: expectedClock,
            SideToMove: GameColor.White,
            InitialFen: _state.InitialFen,
            MoveHistory: [],
            DrawState: new DrawState(),
            MoveOptions: new(
                LegalMoves: legalMoves.MovePaths,
                HasForcedMoves: legalMoves.HasForcedMoves
            )
        );
        result.Value.Should().BeEquivalentTo(expectedGameState);
    }

    [Fact]
    public async Task RequestDrawAsync_sends_notification_if_no_pending_request()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.RequestDrawAsync(_whitePlayer.UserId);
        result.IsError.Should().BeFalse();

        await _gameNotifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                _gameToken,
                new DrawState(ActiveRequester: GameColor.White)
            );

        var state = await grain.GetStateAsync();
        state.Value.DrawState.ActiveRequester.Should().Be(GameColor.White);
        _stateStats.Writes.Should().Be(1);
    }

    [Fact]
    public async Task RequestDrawAsync_ends_the_game_if_there_is_a_pending_request()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        await grain.RequestDrawAsync(_whitePlayer.UserId);
        await grain.RequestDrawAsync(_blackPlayer.UserId);

        await TestGameEndedAsync(grain, _gameResultDescriber.DrawByAgreement());
    }

    [Fact]
    public async Task DeclineDrawAsync_declines_the_draw_correctly()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        await grain.RequestDrawAsync(_whitePlayer.UserId);
        await grain.DeclineDrawAsync(_blackPlayer.UserId);

        await _gameNotifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                _gameToken,
                new DrawState(WhiteCooldown: _settings.DrawCooldown)
            );

        var state = await grain.GetStateAsync();
        state.Value.DrawState.ActiveRequester.Should().BeNull();
        _stateStats.Writes.Should().Be(2);
        _stateStats.Clears.Should().Be(0);
    }

    [Fact]
    public async Task MovePieceAsync_with_a_valid_move_creates_a_correct_move_made_notification()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);
        var in2Seconds = _fakeNow + TimeSpan.FromSeconds(2);
        _timeProviderMock.GetUtcNow().Returns(in2Seconds);

        var move = await MakeLegalMoveAsync(grain, _whitePlayer);

        var expectedTimeLeft =
            _pool.TimeControl.BaseSeconds * 1000
            + _pool.TimeControl.IncrementSeconds * 1000 // add increment
            - 2 * 1000; // removed elapsed time

        MoveSnapshot expectedMoveSnapshot = new(
            Path: MovePath.FromMove(move, GameConstants.BoardWidth),
            San: _sanCalculator.CalculateSan(
                move,
                _gameCore.GetLegalMovesOf(GameColor.White, _state.Core).AllMoves
            ),
            TimeLeft: expectedTimeLeft
        );
        ClockSnapshot expectedClock = new(
            WhiteClock: expectedTimeLeft,
            BlackClock: _pool.TimeControl.BaseSeconds * 1000,
            LastUpdated: in2Seconds.ToUnixTimeMilliseconds()
        );
        var legalMoves = _gameCore.GetLegalMovesOf(GameColor.Black, _state.Core);
        await _gameNotifierMock
            .Received(1)
            .NotifyMoveMadeAsync(
                gameToken: _gameToken,
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
    public async Task MovePieceAsync_with_an_invalid_move_should_returns_an_error()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        var result = await grain.MovePieceAsync(
            _whitePlayer.UserId,
            new(from: new AlgebraicPoint("e2"), to: new AlgebraicPoint("e8"))
        );
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public async Task MovePieceAsync_that_results_in_game_over_ends_the_game()
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
            _gameNotifierMock.ClearReceivedCalls();
            await grain.MovePieceAsync(_blackPlayer.UserId, new(blackFrom, blackTo));
        }

        // (1 white move + 1 black move) * 4 times - 1 that results in a draw
        _stateStats.Writes.Should().Be(2 * 4 - 1);
        await TestGameEndedAsync(grain, _gameResultDescriber.ThreeFold());
        // make sure the state was not deleted before the notification
        await _gameNotifierMock
            .Received(1)
            .NotifyMoveMadeAsync(
                gameToken: _gameToken,
                move: Arg.Any<MoveSnapshot>(),
                moveNumber: 2 * 4,
                clocks: Arg.Any<ClockSnapshot>(),
                sideToMove: Arg.Any<GameColor>(),
                sideToMoveUserId: Arg.Any<UserId>(),
                encodedLegalMoves: Arg.Any<IEnumerable<byte>>(),
                hasForcedMoves: Arg.Any<bool>()
            );
    }

    [Fact]
    public async Task MovePieceAsync_handles_forced_moves()
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
                Arg.Any<GameToken>(),
                Arg.Any<MoveSnapshot>(),
                Arg.Any<int>(),
                Arg.Any<ClockSnapshot>(),
                Arg.Any<GameColor>(),
                Arg.Any<UserId>(),
                Arg.Any<byte[]>(),
                hasForcedMoves: true
            );

        var state = await grain.GetStateAsync(_whitePlayer.UserId);
        state.Value.MoveOptions.HasForcedMoves.Should().BeTrue();
        state.Value.MoveOptions.LegalMoves.Should().HaveCount(1);
    }

    [Fact]
    public async Task MovePieceAsync_decrements_draw_cooldown()
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
            .NotifyDrawStateChangeAsync(Arg.Any<GameToken>(), Arg.Any<DrawState>());

        var state = await grain.GetStateAsync();
        state.Value.DrawState.WhiteCooldown.Should().Be(drawCooldown - 1);
        state.Value.DrawState.BlackCooldown.Should().Be(0);
    }

    [Fact]
    public async Task MovePieceAsync_declines_pending_draw_request()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        await grain.RequestDrawAsync(_whitePlayer.UserId);

        await MakeLegalMoveAsync(grain, _whitePlayer);
        await MakeLegalMoveAsync(grain, _blackPlayer);

        await _gameNotifierMock
            .Received(1)
            .NotifyDrawStateChangeAsync(
                _gameToken,
                new DrawState(WhiteCooldown: _settings.DrawCooldown)
            );

        var state = await grain.GetStateAsync();
        state.Value.DrawState.ActiveRequester.Should().BeNull();
    }

    [Fact]
    public async Task EndGameAsync_aborts_the_game_if_not_enough_moves_have_been_made()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        // No moves or just one move = still abortable
        await grain.RequestGameEndAsync(_whitePlayer.UserId);

        await TestGameEndedAsync(grain, _gameResultDescriber.Aborted(GameColor.White));
    }

    [Fact]
    public async Task EndGameAsync_resigns_the_game_after_abortion_threshold_is_passed()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain);

        // make enough moves to exceed abort threshold
        await MakeLegalMoveAsync(grain, _whitePlayer);
        await MakeLegalMoveAsync(grain, _blackPlayer);
        await MakeLegalMoveAsync(grain, _whitePlayer);

        _stateStats.ResetCounts();
        await grain.RequestGameEndAsync(_whitePlayer.UserId);
        _stateStats.Writes.Should().Be(0);

        await TestGameEndedAsync(grain, _gameResultDescriber.Resignation(GameColor.White));
    }

    [Fact]
    public async Task TickClock_ends_the_game_when_time_runs_out()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain, timeControl: new(0, 0));

        await Silo.FireAllTimersAsync();

        await TestGameEndedAsync(grain, _gameResultDescriber.Timeout(GameColor.White));
    }

    [Fact]
    public async Task TickClock_doesnt_end_the_game_when_not_necessary()
    {
        var grain = await CreateGrainAsync();
        await StartGameAsync(grain, timeControl: new(BaseSeconds: 10, 0));

        await Silo.FireAllTimersAsync();

        (await grain.IsGameOngoingAsync()).Should().BeTrue();
    }

    private Move GetLegalMoveFor(GamePlayer player) =>
        _gameCore.GetLegalMovesOf(player.Color, _state.Core).MoveMap.First().Value;

    private async Task<Move> MakeLegalMoveAsync(IGameGrain grain, GamePlayer player)
    {
        var move = GetLegalMoveFor(player);
        await grain.MovePieceAsync(player.UserId, key: new(move.From, move.To));
        return move;
    }

    private async Task StartGameAsync(
        IGameGrain grain,
        GamePlayer? whitePlayer = null,
        GamePlayer? blackPlayer = null,
        TimeControlSettings? timeControl = null,
        PoolType? poolType = null
    )
    {
        await grain.StartGameAsync(
            whitePlayer: whitePlayer ?? _whitePlayer,
            blackPlayer: blackPlayer ?? _blackPlayer,
            pool: new PoolKey(
                PoolType: poolType ?? _pool.PoolType,
                TimeControl: timeControl ?? _pool.TimeControl
            )
        );
        _stateStats.ResetCounts();
    }

    private async Task TestGameEndedAsync(IGameGrain grain, GameEndStatus expectedEndStatus)
    {
        await _gameNotifierMock
            .Received(1)
            .NotifyGameEndedAsync(
                _gameToken,
                ArgEx.FluentAssert<GameResultData>(
                    (x) =>
                    {
                        x?.Result.Should().Be(expectedEndStatus.Result);
                        x?.ResultDescription.Should().Be(expectedEndStatus.ResultDescription);
                    }
                )
            );
        var isOngoing = await grain.IsGameOngoingAsync();
        isOngoing.Should().BeFalse();

        _stateStats.Clears.Should().Be(1);
    }
}
