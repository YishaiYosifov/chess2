using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame;
using Chess2.Api.LiveGame.Errors;
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
            _drawEvaluatorMock
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

        var whiteMoves = _gameCore.GetLegalMovesFor(GameColor.White);
        var blackMoves = _gameCore.GetLegalMovesFor(GameColor.Black);

        whiteMoves.Moves.Should().HaveCount(2);
        whiteMoves.Moves.Should().ContainKey((m1.From, m1.To)).WhoseValue.Should().Be(m1);
        whiteMoves.Moves.Should().ContainKey((m2.From, m2.To)).WhoseValue.Should().Be(m2);
        whiteMoves.MovePaths.Should().BeEquivalentTo(movePaths);
        whiteMoves.EncodedMoves.Should().BeEquivalentTo(movesEnc);

        blackMoves.Moves.Should().BeEmpty();
        blackMoves.MovePaths.Should().BeEmpty();
        blackMoves.EncodedMoves.Should().BeEmpty();

        _gameCore.Fen.Should().Be("fen");
        _drawEvaluatorMock.Received(1).RegisterInitialPosition("fen");
    }

    [Fact]
    public void GetLegalMovesFor_returns_empty_when_uninitialized()
    {
        _gameCore.GetLegalMovesFor(GameColor.Black).Should().BeEquivalentTo(new LegalMoveSet());
        _gameCore.GetLegalMovesFor(GameColor.White).Should().BeEquivalentTo(new LegalMoveSet());
    }

    [Fact]
    public void MakeMove_returns_an_error_when_provided_an_invalid_move()
    {
        AlgebraicPoint from = new("e2");
        AlgebraicPoint to = new("e4");

        var result = _gameCore.MakeMove(from, to, GameColor.White);

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public void MakeMove_moves_the_piece_and_updates_legal_moves()
    {
        Move move = new(new("e2"), new("e4"), PieceFactory.White());
        MovePath path = MovePath.FromMove(move, GameConstants.BoardWidth);
        byte[] encoded = [1, 2, 3];
        string san = "e4";

        SetupLegalMove(move, encoded, san);
        _fenCalculatorMock.CalculateFen(Arg.Any<ChessBoard>()).Returns("updated-fen");

        _gameCore.InitializeGame();

        var result = _gameCore.MakeMove(move.From, move.To, GameColor.White);

        result.IsError.Should().BeFalse();
        result
            .Value.Should()
            .Be(new MoveResult(Move: move, MovePath: path, San: san, EndStatus: null));

        _gameCore.Fen.Should().Be("updated-fen");
        _gameCore.SideToMove.Should().Be(GameColor.Black);
    }

    [Fact]
    public void MakeMove_alternates_when_making_a_move()
    {
        Move move = new(new("e2"), new("e4"), PieceFactory.White());
        SetupLegalMove(move);

        _gameCore.InitializeGame();

        _gameCore.SideToMove.Should().Be(GameColor.White);
        _gameCore.MakeMove(move.From, move.To, GameColor.White);
        _gameCore.SideToMove.Should().Be(GameColor.Black);
    }

    [Fact]
    public void MakeMove_sets_end_status_when_move_results_in_draw()
    {
        Move move = new(new("e2"), new("e4"), PieceFactory.White());
        string fen = "some fen";
        GameEndStatus drawStatus = new(GameResult.Draw, "test draw reason");
        SetupLegalMove(move);

        _fenCalculatorMock.CalculateFen(Arg.Any<ChessBoard>()).Returns(fen);
        _drawEvaluatorMock
            .TryEvaluateDraw(move, fen, out Arg.Any<GameEndStatus?>())
            .Returns(ci =>
            {
                ci[2] = drawStatus;
                return true;
            });
        _gameCore.InitializeGame();

        var result = _gameCore.MakeMove(move.From, move.To, GameColor.White);

        result.IsError.Should().BeFalse();
        result.Value.EndStatus.Should().Be(drawStatus);
    }

    private void SetupLegalMove(Move? move = null, byte[]? encoded = null, string? san = null)
    {
        move ??= new Move(new("e2"), new("e4"), PieceFactory.White());
        encoded ??= [1, 2, 3];
        san ??= "e4";

        _legalMoveCalculatorMock
            .CalculateAllLegalMoves(Arg.Any<ChessBoard>(), GameColor.White)
            .Returns([move]);

        MovePath[] expectedPaths = [MovePath.FromMove(move, GameConstants.BoardWidth)];
        _encoderMock
            .EncodeMoves(Arg.Is<IEnumerable<MovePath>>(paths => paths.SequenceEqual(expectedPaths)))
            .Returns(encoded);

        Move[] legalMoves = [move];
        _sanCalculatorMock
            .CalculateSan(move, Arg.Is<IEnumerable<Move>>(moves => moves.SequenceEqual(legalMoves)))
            .Returns(san);
    }
}
