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
    public void InitializeGame_sets_the_initial_state_correctly()
    {
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

        _gameCore.InitializeGame();

        var whiteMoves = _gameCore.GetLegalMoves(GameColor.White);
        var blackMoves = _gameCore.GetLegalMoves(GameColor.Black);

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

        _gameCore.InitialFen.Should().Be("fen");
        _drawEvaluatorMock.Received(1).RegisterInitialPosition("fen");
    }

    [Fact]
    public void GetLegalMoves_returns_empty_when_uninitialized()
    {
        _gameCore.GetLegalMoves(GameColor.Black).Should().BeEquivalentTo(new LegalMoveSet());
        _gameCore.GetLegalMoves(GameColor.White).Should().BeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void MakeMove_returns_an_error_when_provided_an_invalid_move()
    {
        AlgebraicPoint from = new("e2");
        AlgebraicPoint to = new("e4");

        var result = _gameCore.MakeMove(new(from, to), GameColor.White);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public void MakeMove_returns_the_correct_MoveResult()
    {
        var move = new Move(new("e2"), new("e4"), PieceFactory.White());
        var key = new MoveKey(From: new("e2"), To: new("e4"));

        List<Move> expectedMoves = [move];
        _legalMoveCalculatorMock
            .CalculateAllLegalMoves(Arg.Any<ChessBoard>(), GameColor.White)
            .Returns(expectedMoves);
        _fenCalculatorMock.CalculateFen(Arg.Any<ChessBoard>()).Returns("fen-string");
        _sanCalculatorMock
            .CalculateSan(move, Arg.Is<IEnumerable<Move>>(x => x.Count() == 0))
            .Returns("e4");

        _gameCore.InitializeGame();

        var result = _gameCore.MakeMove(key, GameColor.White);

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
        var whiteMove = new Move(new("e2"), new("e4"), PieceFactory.White());
        var key = new MoveKey(From: new("e2"), To: new("e4"));

        var blackMove1 = new Move(new("e7"), new("e5"), PieceFactory.Black());
        var blackMove2 = new Move(new("d7"), new("d5"), PieceFactory.Black());

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

        _gameCore.InitializeGame();
        _gameCore.MakeMove(key, GameColor.White);

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

        _gameCore.LegalMoves.Should().BeEquivalentTo(expectedLegalMoves);
        _gameCore.SideToMove.Should().Be(GameColor.Black);
    }
}
