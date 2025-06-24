using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.TestKit;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameTests;

public class GameActorTests : BaseActorTest
{
    private const string TestGameToken = "testtoken";
    private readonly TimeControlSettings _timeControl = new(600, 5);

    private readonly IGameCore _gameMock = Substitute.For<IGameCore>();

    private readonly IActorRef _gameActor;
    private readonly TestProbe _probe;

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).Generate();
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).Generate();

    public GameActorTests()
    {
        _gameActor = Sys.ActorOf(Props.Create(() => new GameActor(TestGameToken, _gameMock)));
        _probe = CreateTestProbe();
    }

    [Fact]
    public async Task GetGameStatus_before_game_started_should_return_NotStarted_and_passivate()
    {
        var parentActor = CreateTestProbe();
        var gameActor = parentActor.ChildActorOf(
            Props.Create(() => new GameActor(TestGameToken, _gameMock)),
            cancellationToken: CT
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
        await StartGameAsync(_whitePlayer, _blackPlayer);
        _gameMock.Received(1).InitializeGame();
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
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task GetGameState_valid_user_should_return_GameStateEvent()
    {
        _gameMock.GetEncodedLegalMovesFor(GameColor.White).Returns(["e2e4"]);
        _gameMock.Fen.Returns("some-fen");
        _gameMock.EncodedMoveHistory.Returns(["e2e4"]);

        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, _whitePlayer.UserId), _probe);
        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.GameStateEvent>>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        result.IsError.Should().BeFalse();
        result.Value.State.SideToMove.Should().Be(GameColor.White);
        result.Value.State.Fen.Should().Be("some-fen");
        result.Value.State.MoveHistory.Should().ContainSingle("e2e4");
        result.Value.State.LegalMoves.Should().ContainSingle("e2e4");
        result.Value.State.TimeControl.Should().Be(_timeControl);
    }

    [Fact]
    public async Task MovePiece_wrong_player_should_return_PlayerInvalid_error()
    {
        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(
            new GameCommands.MovePiece(
                TestGameToken,
                "invalid",
                new AlgebraicPoint("a2"),
                new AlgebraicPoint("c4")
            ),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.PieceMoved>>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task MovePiece_valid_should_send_PieceMoved()
    {
        var newWhiteLegalMoves = new List<string> { "e2e4" };
        var newBlackLegalMoves = new List<string> { "e7e5" };
        const string encodedMove = "e2e4";
        const int moveNumber = 69;
        _gameMock.MakeMove(new AlgebraicPoint("e2"), new AlgebraicPoint("e4")).Returns(encodedMove);
        _gameMock.GetEncodedLegalMovesFor(GameColor.White).Returns(newWhiteLegalMoves);
        _gameMock.GetEncodedLegalMovesFor(GameColor.Black).Returns(newBlackLegalMoves);
        _gameMock.MoveNumber.Returns(moveNumber);

        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(
            new GameCommands.MovePiece(
                TestGameToken,
                _whitePlayer.UserId,
                new AlgebraicPoint("e2"),
                new AlgebraicPoint("e4")
            ),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.PieceMoved>>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.IsError.Should().BeFalse();
        result
            .Value.Should()
            .BeEquivalentTo(
                new GameEvents.PieceMoved(
                    encodedMove,
                    WhiteLegalMoves: newWhiteLegalMoves,
                    WhiteId: _whitePlayer.UserId,
                    BlackLegalMoves: newBlackLegalMoves,
                    BlackId: _blackPlayer.UserId,
                    SideToMove: GameColor.White, // because we are mocking game core, it doesn't change side to move
                    MoveNumber: moveNumber
                )
            );
    }

    [Fact]
    public async Task MovePiece_invalid_should_return_error()
    {
        _gameMock
            .MakeMove(new AlgebraicPoint("e2"), new AlgebraicPoint("e4"))
            .Returns(GameErrors.MoveInvalid);

        await StartGameAsync(_whitePlayer, _blackPlayer);

        _gameActor.Tell(
            new GameCommands.MovePiece(
                TestGameToken,
                _whitePlayer.UserId,
                new AlgebraicPoint("e2"),
                new AlgebraicPoint("e4")
            ),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<GameEvents.PieceMoved>>(
            cancellationToken: CT
        );
        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.MoveInvalid);
    }

    private async Task StartGameAsync(GamePlayer whitePlayer, GamePlayer blackPlayer)
    {
        _gameActor.Tell(
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
