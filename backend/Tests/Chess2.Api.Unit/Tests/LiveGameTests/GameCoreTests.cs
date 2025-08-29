using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameCoreTests
{
    private readonly IFenCalculator _fenCalculatorMock = Substitute.For<IFenCalculator>();
    private readonly ILegalMoveCalculator _legalMoveCalculatorMock =
        Substitute.For<ILegalMoveCalculator>();
    private readonly IMoveEncoder _encoderMock = Substitute.For<IMoveEncoder>();
    private readonly ISanCalculator _sanCalculatorMock = Substitute.For<ISanCalculator>();
    private readonly IDrawEvaulator _drawEvaluatorMock = Substitute.For<IDrawEvaulator>();

    private readonly GameCore _gameCore;

    public GameCoreTests()
    {
        _gameCore = new(
            Substitute.For<ILogger<GameCore>>(),
            _fenCalculatorMock,
            _legalMoveCalculatorMock,
            _encoderMock,
            _sanCalculatorMock,
            _drawEvaluatorMock,
            new GameResultDescriber()
        );
    }

    [Fact]
    public void StartGame_sets_the_initial_state_correctly()
    {
        GameCoreState state = new();
        Move m1 = new(new("e2"), new("e4"), PieceFactory.White());
        Move m2 = new(new("g1"), new("f3"), PieceFactory.White());
        Move[] allMoves = [m1, m2];
        MovePath[] movePaths =
        [
            MovePath.FromMove(m1, GameConstants.BoardWidth),
            MovePath.FromMove(m2, GameConstants.BoardWidth),
        ];
        byte[] movesEnc = [1, 2, 3];

        _legalMoveCalculatorMock
            .CalculateAllLegalMoves(Arg.Any<ChessBoard>(), GameColor.White)
            .Returns(allMoves);
        _encoderMock
            .EncodeMoves(Arg.Is<IEnumerable<MovePath>>(m => m.SequenceEqual(movePaths)))
            .Returns(movesEnc);
        _fenCalculatorMock.CalculateFen(Arg.Any<ChessBoard>()).Returns("fen");

        var initialFen = _gameCore.StartGame(state);

        var whiteMoves = _gameCore.GetLegalMovesOf(GameColor.White, state);
        var blackMoves = _gameCore.GetLegalMovesOf(GameColor.Black, state);

        whiteMoves.MovesMap.Should().HaveCount(2);
        whiteMoves
            .MovesMap.Should()
            .ContainKey(new MoveKey(m1.From, m1.To))
            .WhoseValue.Should()
            .Be(m1);
        whiteMoves
            .MovesMap.Should()
            .ContainKey(new MoveKey(m2.From, m2.To))
            .WhoseValue.Should()
            .Be(m2);
        whiteMoves.MovePaths.Should().BeEquivalentTo(movePaths);
        whiteMoves.EncodedMoves.Should().BeEquivalentTo(movesEnc);
        whiteMoves.HasForcedMoves.Should().BeFalse();

        blackMoves.MovesMap.Should().BeEmpty();
        blackMoves.MovePaths.Should().BeEmpty();
        blackMoves.EncodedMoves.Should().BeEmpty();

        initialFen.Should().Be("fen");
        _drawEvaluatorMock.Received(1).RegisterInitialPosition("fen", state.AutoDrawState);
    }

    [Fact]
    public void GetLegalMoves_returns_empty_when_uninitialized()
    {
        GameCoreState state = new();

        _gameCore
            .GetLegalMovesOf(GameColor.Black, state)
            .Should()
            .BeEquivalentTo(new LegalMoveSet());
        _gameCore
            .GetLegalMovesOf(GameColor.White, state)
            .Should()
            .BeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void GetLegalMoves_returns_empty_when_not_side_to_move()
    {
        GameCoreState state = new();

        _gameCore.StartGame(state);
        _gameCore
            .GetLegalMovesOf(GameColor.Black, state)
            .Should()
            .BeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void MakeMove_returns_an_error_when_provided_an_invalid_move()
    {
        GameCoreState state = new();
        AlgebraicPoint from = new("e2");
        AlgebraicPoint to = new("e4");
        var key = new MoveKey(from, to);

        var result = _gameCore.MakeMove(key, state);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public void MakeMove_returns_the_correct_MoveResult()
    {
        GameCoreState state = new();
        Move move = new(new("e2"), new("e4"), PieceFactory.White());
        MoveKey key = new(From: new("e2"), To: new("e4"));

        List<Move> expectedMoves = [move];
        _legalMoveCalculatorMock
            .CalculateAllLegalMoves(Arg.Any<ChessBoard>(), GameColor.White)
            .Returns(expectedMoves);
        _fenCalculatorMock.CalculateFen(Arg.Any<ChessBoard>()).Returns("fen-string");
        _sanCalculatorMock
            .CalculateSan(move, Arg.Is<IEnumerable<Move>>(x => x.Count() == 1), false)
            .Returns("e4");

        _gameCore.StartGame(state);

        var result = _gameCore.MakeMove(key, state);

        result.IsError.Should().BeFalse();
        MoveResult expected = new(
            Move: move,
            MovePath: MovePath.FromMove(move, GameConstants.BoardWidth),
            San: "e4",
            EndStatus: null
        );
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void MakeMove_updates_the_correct_legal_moves()
    {
        GameCoreState state = new();
        Move whiteMove = new(new("e2"), new("e4"), PieceFactory.White());
        MoveKey key = new(From: new("e2"), To: new("e4"));

        Move blackMove1 = new(new("e7"), new("e5"), PieceFactory.Black());
        Move blackMove2 = new(new("d7"), new("d5"), PieceFactory.Black());

        List<Move> whiteMoves = [whiteMove];
        List<Move> blackMoves = [blackMove1, blackMove2];
        List<MovePath> blackMovePaths =
        [
            MovePath.FromMove(blackMove1, GameConstants.BoardWidth),
            MovePath.FromMove(blackMove2, GameConstants.BoardWidth),
        ];
        byte[] encodedBlackMoves = [1, 2, 3];

        _legalMoveCalculatorMock
            .CalculateAllLegalMoves(Arg.Any<ChessBoard>(), GameColor.White)
            .Returns(whiteMoves);

        _legalMoveCalculatorMock
            .CalculateAllLegalMoves(Arg.Any<ChessBoard>(), GameColor.Black)
            .Returns(blackMoves);

        _encoderMock
            .EncodeMoves(Arg.Is<IEnumerable<MovePath>>(x => x.SequenceEqual(blackMovePaths)))
            .Returns(encodedBlackMoves);

        _fenCalculatorMock.CalculateFen(Arg.Any<ChessBoard>()).Returns("fen-after-move");

        _gameCore.StartGame(state);
        _gameCore.MakeMove(key, state);

        Dictionary<MoveKey, Move> expectedMovesMap = new()
        {
            [new MoveKey(blackMove1.From, blackMove1.To)] = blackMove1,
            [new MoveKey(blackMove2.From, blackMove2.To)] = blackMove2,
        };

        LegalMoveSet expectedLegalMoves = new(
            MovesMap: expectedMovesMap,
            MovePaths: blackMovePaths,
            EncodedMoves: encodedBlackMoves,
            HasForcedMoves: false
        );

        state.LegalMoves.Should().BeEquivalentTo(expectedLegalMoves);
        _gameCore.SideToMove(state).Should().Be(GameColor.Black);
    }
}
