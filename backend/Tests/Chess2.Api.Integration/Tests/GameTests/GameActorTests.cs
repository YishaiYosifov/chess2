using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.TestKit;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.GameTests;

public class GameActorTests : BaseAkkaIntegrationTest
{
    private const string TestGameToken = "testtoken";
    private readonly TimeControlSettings _timeControl = new(600, 5);

    private readonly IMoveEncoder _moveEncoder;
    private readonly IGameCore _gameCore;
    private readonly IActorRef _gameActor;
    private readonly TestProbe _probe;

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    public GameActorTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _moveEncoder = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IMoveEncoder>();
        _gameCore = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameActor = Sys.ActorOf(Props.Create(() => new GameActor(TestGameToken, _gameCore)));
        _probe = CreateTestProbe();
    }

    [Fact]
    public async Task GetGameStatus_before_game_started_should_return_NotStarted_and_passivate()
    {
        var parentActor = CreateTestProbe();
        var gameActor = parentActor.ChildActorOf(
            Props.Create(() => new GameActor(TestGameToken, _gameCore)),
            cancellationToken: ApiTestBase.CT
        );

        gameActor.Tell(new GameQueries.GetGameStatus(TestGameToken), _probe);

        var result = await _probe.ExpectMsgAsync<GameEvents.GameStatusEvent>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.Status.Should().Be(GameStatus.NotStarted);

        var passivate = await parentActor.ExpectMsgAsync<Passivate>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        passivate.StopMessage.Should().Be(PoisonPill.Instance);
    }

    [Fact]
    public async Task StartGame_should_initialize_game_and_transition_to_playing_state()
    {
        var statusEvent = await _gameActor.Ask<GameEvents.GameStatusEvent>(
            new GameQueries.GetGameStatus(TestGameToken),
            ApiTestBase.CT
        );
        statusEvent.Status.Should().Be(GameStatus.NotStarted);

        await StartGameAsync(_whitePlayer, _blackPlayer);

        statusEvent = await _gameActor.Ask<GameEvents.GameStatusEvent>(
            new GameQueries.GetGameStatus(TestGameToken),
            ApiTestBase.CT
        );
        statusEvent.Status.Should().Be(GameStatus.OnGoing);
    }

    [Fact]
    public async Task GetGameState_invalid_user_should_return_player_invalid_error()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(
            new GameQueries.GetGameState(TestGameToken, ForUserId: "invalid-user"),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.GameStateEvent>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task GetGameState_valid_user_should_return_GameStateEvent()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.GameStateEvent>>(
            cancellationToken: ApiTestBase.CT
        );

        result.IsError.Should().BeFalse();
        var expectedGameState = new GameState(
            WhitePlayer: _whitePlayer,
            BlackPlayer: _blackPlayer,
            SideToMove: GameColor.White,
            Fen: _gameCore.Fen,
            MoveHistory: _gameCore.EncodedMoveHistory,
            LegalMoves: _gameCore.GetEncodedLegalMovesFor(GameColor.White),
            TimeControl: _timeControl
        );
        result.Value.State.Should().BeEquivalentTo(expectedGameState);
    }

    [Fact]
    public async Task MovePiece_wrong_player_should_return_PlayerInvalid_error()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(
            new GameCommands.MovePiece(
                TestGameToken,
                _blackPlayer.UserId,
                new AlgebraicPoint("a2"),
                new AlgebraicPoint("c4")
            ),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.PieceMoved>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task MovePiece_valid_should_send_PieceMoved()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        var (result, move) = await MakeLegalMoveAsync(_whitePlayer);

        var expectedEncodedMove = _moveEncoder.EncodeSingleMove(move);
        var expectedMove = new GameEvents.PieceMoved(
            Move: expectedEncodedMove,
            WhiteLegalMoves: _gameCore.GetEncodedLegalMovesFor(GameColor.White),
            WhiteId: _whitePlayer.UserId,
            BlackLegalMoves: _gameCore.GetEncodedLegalMovesFor(GameColor.Black),
            BlackId: _blackPlayer.UserId,
            SideToMove: GameColor.Black,
            MoveNumber: 1
        );
        result.Value.Should().BeEquivalentTo(expectedMove);
    }

    [Fact]
    public async Task MovePiece_invalid_should_return_error()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(
            new GameCommands.MovePiece(
                TestGameToken,
                _whitePlayer.UserId,
                new AlgebraicPoint("e2"),
                new AlgebraicPoint("e8")
            ),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.PieceMoved>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public async Task EndGame_invalid_user_should_return_PlayerInvalid()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, "nonexistent-user"), _probe);

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.GameEnded>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task EndGame_valid_user_should_return_Aborted_if_early_game()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        // No moves or just one move = still abortable
        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.GameEnded>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeFalse();
        result.Value.Result.Should().Be(GameResult.Aborted);
    }

    [Fact]
    public async Task EndGame_valid_user_should_return_win_for_opponent_after_early_moves()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        // Make enough moves to exceed abort threshold
        await MakeLegalMoveAsync(_whitePlayer);
        await MakeLegalMoveAsync(_blackPlayer);
        await MakeLegalMoveAsync(_whitePlayer);

        // Now White resigns — Black should win
        _gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.GameEnded>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeFalse();
        result.Value.Result.Should().Be(GameResult.BlackWin);
    }

    [Fact]
    public async Task EndGame_should_passivate_actor()
    {
        var parentProbe = CreateTestProbe();
        var gameActor = parentProbe.ChildActorOf(
            Props.Create(() => new GameActor(TestGameToken, _gameCore)),
            cancellationToken: ApiTestBase.CT
        );

        await StartGameAsync(_whitePlayer, _blackPlayer, gameActor);

        gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);
        var ended = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.GameEnded>>(
            cancellationToken: ApiTestBase.CT
        );

        var passivate = await parentProbe.ExpectMsgAsync<Passivate>(
            cancellationToken: ApiTestBase.CT
        );
        passivate.StopMessage.Should().Be(PoisonPill.Instance);
    }

    private async Task<(ErrorOr<GameEvents.PieceMoved> result, Move move)> MakeLegalMoveAsync(
        GamePlayer player,
        IActorRef? gameActor = null
    )
    {
        gameActor ??= _gameActor;
        var move = _gameCore.GetLegalMovesFor(player.Color).First();
        var movedFrom = move.Key.from;
        var movedTo = move.Key.to;
        gameActor.Tell(
            new GameCommands.MovePiece(TestGameToken, player.UserId, movedFrom, movedTo),
            _probe
        );

        var moveResult = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.PieceMoved>>(
            cancellationToken: ApiTestBase.CT,
            duration: TimeSpan.FromSeconds(10)
        );
        moveResult.IsError.Should().BeFalse();
        return (moveResult, move.Value);
    }

    private async Task StartGameAsync(
        GamePlayer whitePlayer,
        GamePlayer blackPlayer,
        IActorRef? gameActor = null
    )
    {
        gameActor ??= _gameActor;
        gameActor.Tell(
            new GameCommands.StartGame(
                TestGameToken,
                WhitePlayer: whitePlayer,
                BlackPlayer: blackPlayer,
                TimeControl: _timeControl
            ),
            _probe.Ref
        );
        await _probe.ExpectMsgAsync<GameEvents.GameStartedEvent>(
            cancellationToken: TestContext.Current.CancellationToken
        );
    }
}
