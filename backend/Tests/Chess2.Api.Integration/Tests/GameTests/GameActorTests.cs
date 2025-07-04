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
using NSubstitute;

namespace Chess2.Api.Integration.Tests.GameTests;

public class GameActorTests : BaseAkkaIntegrationTest
{
    private const string TestGameToken = "testtoken";
    private readonly TimeControlSettings _timeControl = new(600, 5);

    private readonly IGameResultDescriber _gameResultDescriber;
    private readonly IMoveEncoder _moveEncoder;
    private readonly IGameCore _gameCore;
    private readonly IActorRef _gameActor;
    private readonly TestProbe _probe;

    private readonly IGameNotifier _gameNotifierMock = Substitute.For<IGameNotifier>();

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    public GameActorTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _moveEncoder = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IMoveEncoder>();
        _gameCore = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameCore>();
        _gameResultDescriber =
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IGameResultDescriber>();
        _gameActor = Sys.ActorOf(
            Props.Create(
                () =>
                    new GameActor(
                        TestGameToken,
                        ApiTestBase.Scope.ServiceProvider,
                        _gameCore,
                        _gameResultDescriber,
                        _gameNotifierMock
                    )
            )
        );
        _probe = CreateTestProbe();
    }

    [Fact]
    public async Task IsGameOngoing_before_game_started_should_return_false_and_passivate()
    {
        var parentActor = CreateTestProbe();
        var gameActor = parentActor.ChildActorOf(
            Props.Create(
                () =>
                    new GameActor(
                        TestGameToken,
                        ApiTestBase.Scope.ServiceProvider,
                        _gameCore,
                        _gameResultDescriber,
                        _gameNotifierMock
                    )
            ),
            cancellationToken: ApiTestBase.CT
        );

        gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);

        var result = await _probe.ExpectMsgAsync<bool>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.Should().BeFalse();

        var passivate = await parentActor.ExpectMsgAsync<Passivate>(
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

        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(new GameQueries.IsGameOngoing(TestGameToken), _probe);
        isOngoing = await _probe.ExpectMsgAsync<bool>(cancellationToken: ApiTestBase.CT);
        isOngoing.Should().BeTrue();
    }

    [Fact]
    public async Task GetGameState_invalid_user_should_return_player_invalid_error()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

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
        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
        var result = await _probe.ExpectMsgAsync<GameEvents.GameStateEvent>(
            cancellationToken: ApiTestBase.CT
        );

        var expectedGameState = new GameState(
            WhitePlayer: _whitePlayer,
            BlackPlayer: _blackPlayer,
            SideToMove: GameColor.White,
            Fen: _gameCore.Fen,
            MoveHistory: _gameCore.EncodedMoveHistory,
            LegalMoves: _gameCore.GetLegalMovesFor(GameColor.White).EncodedMoves,
            TimeControl: _timeControl
        );
        result.State.Should().BeEquivalentTo(expectedGameState);
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

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(
            cancellationToken: ApiTestBase.CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task MovePiece_valid_should_send_PieceMoved()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        var move = await MakeLegalMoveAsync(_whitePlayer);

        var expectedEncodedMove = _moveEncoder.EncodeSingleMove(move);
        await _gameNotifierMock
            .Received(1)
            .NotifyMoveMadeAsync(
                gameToken: TestGameToken,
                move: expectedEncodedMove,
                sideToMove: GameColor.Black,
                moveNumber: 1,
                sideToMoveUserId: _blackPlayer.UserId,
                _gameCore.GetLegalMovesFor(GameColor.Black).EncodedMoves
            );
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

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(
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

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(
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

        await _probe.ExpectMsgAsync<GameEvents.GameEnded>(
            TimeSpan.FromDays(1),
            cancellationToken: ApiTestBase.CT
        );

        await _gameNotifierMock
            .Received(1)
            .NotifyGameEndedAsync(
                TestGameToken,
                GameResult.Aborted,
                _gameResultDescriber.Aborted(GameColor.White),
                Arg.Any<int?>(),
                Arg.Any<int?>()
            );
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

        await _probe.ExpectMsgAsync<GameEvents.GameEnded>(cancellationToken: ApiTestBase.CT);

        await _gameNotifierMock
            .Received(1)
            .NotifyGameEndedAsync(
                TestGameToken,
                GameResult.BlackWin,
                _gameResultDescriber.Resignation(GameColor.White),
                Arg.Any<int?>(),
                Arg.Any<int?>()
            );
    }

    [Fact]
    public async Task EndGame_should_passivate_actor()
    {
        var parentProbe = CreateTestProbe();
        var gameActor = parentProbe.ChildActorOf(
            Props.Create(
                () =>
                    new GameActor(
                        TestGameToken,
                        ApiTestBase.Scope.ServiceProvider,
                        _gameCore,
                        _gameResultDescriber,
                        _gameNotifierMock
                    )
            ),
            cancellationToken: ApiTestBase.CT
        );

        await StartGameAsync(_whitePlayer, _blackPlayer, gameActor);

        gameActor.Tell(new GameCommands.EndGame(TestGameToken, _whitePlayer.UserId), _probe);
        var ended = await _probe.ExpectMsgAsync<GameEvents.GameEnded>(
            cancellationToken: ApiTestBase.CT
        );

        var passivate = await parentProbe.ExpectMsgAsync<Passivate>(
            cancellationToken: ApiTestBase.CT
        );
        passivate.StopMessage.Should().Be(PoisonPill.Instance);
    }

    private async Task<Move> MakeLegalMoveAsync(GamePlayer player, IActorRef? gameActor = null)
    {
        gameActor ??= _gameActor;
        var move = _gameCore.GetLegalMovesFor(player.Color).Moves.First();
        var movedFrom = move.Key.from;
        var movedTo = move.Key.to;
        gameActor.Tell(
            new GameCommands.MovePiece(TestGameToken, player.UserId, movedFrom, movedTo),
            _probe
        );

        await _probe.ExpectMsgAsync<GameEvents.PieceMoved>(
            cancellationToken: ApiTestBase.CT,
            duration: TimeSpan.FromSeconds(10)
        );

        return move.Value;
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
