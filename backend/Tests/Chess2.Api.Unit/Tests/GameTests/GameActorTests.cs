using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.TestKit;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.TestActors;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameTests;

public class GameActorTests : BaseUnitTest
{
    private const string TestGameToken = "testtoken";

    private readonly IGame _gameMock = Substitute.For<IGame>();

    private readonly IActorRef _gameActor;
    private readonly TestProbe _probe;

    public GameActorTests()
    {
        _gameActor = Sys.ActorOf(Props.Create(() => new GameActor(TestGameToken, _gameMock)));
        _probe = CreateTestProbe();
    }

    [Fact]
    public async Task GetGameStatus_before_game_started_should_return_NotStarted_and_passivate()
    {
        var parent = Sys.ActorOf(
            Props.Create(
                () =>
                    new ForwardingParentActor<GameActor>(
                        () => new GameActor(TestGameToken, _gameMock),
                        _probe.Ref
                    )
            )
        );
        parent.Tell(new GameQueries.GetGameStatus(TestGameToken), _probe);

        var result = await _probe.ExpectMsgAsync<GameEvents.GameStatusEvent>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.Status.Should().Be(GameStatus.NotStarted);

        var passivate = await _probe.ExpectMsgAsync<Passivate>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        passivate.StopMessage.Should().Be(PoisonPill.Instance);
    }

    [Fact]
    public async Task StartGame_should_initialize_game_and_transition_to_playing_state()
    {
        await StartGameAsync("user1", "user2");
        _gameMock.Received(1).InitializeGame();
    }

    [Fact]
    public async Task GetGameState_invalid_user_should_return_player_invalid_error()
    {
        await StartGameAsync("user1", "user2");

        _gameActor.Tell(
            new GameQueries.GetGameState(TestGameToken, ForUserId: "invalid-user"),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<GameEvents.GameError>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task GetGameState_valid_user_should_return_GameStateEvent()
    {
        _gameMock.GetEncodedLegalMovesFor(GameColor.White).Returns(["e2e4"]);
        _gameMock.Fen.Returns("some-fen");
        _gameMock.EncodedMoveHistory.Returns(["e2e4"]);

        await StartGameAsync("white", "black");

        _gameActor.Tell(new GameQueries.GetGameState(TestGameToken, "white"), _probe);
        var result = await _probe.ExpectMsgAsync<GameEvents.GameStateEvent>(
            cancellationToken: TestContext.Current.CancellationToken
        );

        result.State.CurrentPlayerColor.Should().Be(GameColor.White);
        result.State.Fen.Should().Be("some-fen");
        result.State.MoveHistory.Should().ContainSingle("e2e4");
        result.State.LegalMoves.Should().ContainSingle("e2e4");
    }

    [Fact]
    public async Task MovePiece_wrong_player_should_return_PlayerInvalid_error()
    {
        await StartGameAsync("white", "black");

        _gameActor.Tell(
            new GameCommands.MovePiece(TestGameToken, "invalid", new(1, 2), new(3, 4)),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<GameEvents.GameError>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.PlayerInvalid);
    }

    [Fact]
    public async Task MovePiece_valid_should_send_PieceMoved_and_switch_turn()
    {
        var newWhiteLegalMoves = new List<string> { "e2e4" };
        var newBlackLegalMoves = new List<string> { "e7e5" };
        const string encodedMove = "e2e4";
        _gameMock.MakeMove(new Point(4, 1), new Point(4, 3)).Returns(encodedMove);
        _gameMock.GetEncodedLegalMovesFor(GameColor.White).Returns(newWhiteLegalMoves);
        _gameMock.GetEncodedLegalMovesFor(GameColor.Black).Returns(newBlackLegalMoves);

        await StartGameAsync("white", "black");

        _gameActor.Tell(
            new GameCommands.MovePiece(TestGameToken, "white", new(4, 1), new(4, 3)),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<GameEvents.PieceMoved>(
            cancellationToken: TestContext.Current.CancellationToken
        );
        result
            .Should()
            .BeEquivalentTo(
                new GameEvents.PieceMoved(
                    encodedMove,
                    newWhiteLegalMoves,
                    newBlackLegalMoves,
                    GameColor.Black
                )
            );
    }

    [Fact]
    public async Task MovePiece_invalid_should_return_error()
    {
        _gameMock.MakeMove(new Point(4, 1), new Point(4, 3)).Returns(GameErrors.MoveInvalid);

        await StartGameAsync("white", "black");

        _gameActor.Tell(
            new GameCommands.MovePiece(TestGameToken, "white", new(4, 1), new(4, 3)),
            _probe
        );

        var result = await _probe.ExpectMsgAsync<GameEvents.GameError>(cancellationToken: CT);
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.MoveInvalid);
    }

    private async Task StartGameAsync(string whiteId, string blackId)
    {
        _gameActor.Tell(
            new GameCommands.StartGame(TestGameToken, WhiteId: whiteId, BlackId: blackId),
            _probe.Ref
        );
        await _probe.ExpectMsgAsync<GameEvents.GameStartedEvent>(
            cancellationToken: TestContext.Current.CancellationToken
        );
    }
}
