using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
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
        var m1 = new Move(new("e2"), new("e4"), PieceFactory.White());
        var m2 = new Move(new("g1"), new("f3"), PieceFactory.White());
        var allMoves = new[] { m1, m2 };
        var movesEnc = new[] { "e2e4", "g1f3" };

        _legalMoveCalculatorMock
            .CalculateAllLegalMoves(Arg.Any<ChessBoard>(), GameColor.White)
            .Returns(allMoves);
        _encoderMock
            .EncodeMoves(
                Arg.Is<IEnumerable<Move>>(m => m.All(x => x.Piece.Color == GameColor.White))
            )
            .Returns(movesEnc);
        _encoderMock
            .EncodeMoves(
                Arg.Is<IEnumerable<Move>>(m => m.All(x => x.Piece.Color == GameColor.Black))
            )
            .Returns(["nah", "uh"]);
        _fenCalculatorMock.CalculateFen(Arg.Any<ChessBoard>()).Returns("fen");

        _gameCore.InitializeGame();

        var whiteMoves = _gameCore.GetLegalMovesFor(GameColor.White);
        var blackMoves = _gameCore.GetLegalMovesFor(GameColor.Black);

        whiteMoves.Moves.Should().HaveCount(2);
        whiteMoves.Moves.Should().ContainKey((m1.From, m1.To)).WhoseValue.Should().Be(m1);
        whiteMoves.Moves.Should().ContainKey((m2.From, m2.To)).WhoseValue.Should().Be(m2);
        whiteMoves.EncodedMoves.Should().BeEquivalentTo(movesEnc);

        blackMoves.Moves.Should().BeEmpty();
        blackMoves.EncodedMoves.Should().BeEmpty();

        _gameCore.Fen.Should().Be("fen");
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
        string encoded = "e2e4";
        string san = "e4";

        SetupLegalMove(move, encoded, san);
        _fenCalculatorMock.CalculateFen(Arg.Any<ChessBoard>()).Returns("updated-fen");

        _gameCore.InitializeGame();

        var result = _gameCore.MakeMove(move.From, move.To, GameColor.White);

        result.IsError.Should().BeFalse();
        result
            .Value.Should()
            .Be(new MoveResult(Move: move, EncodedMove: encoded, San: san, EndStatus: null));

        _gameCore.Fen.Should().Be("updated-fen");
        _gameCore.SideToMove.Should().Be(GameColor.Black);
    }

    [Fact]
    public void MakeMove_alternates_when_making_a_move()
    {
        Move move = new(new("e2"), new("e4"), PieceFactory.White());
        SetupLegalMove(move, "e2e4", "e4");

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
        SetupLegalMove(move, "e2e4", "e4");

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

    private void SetupLegalMove(Move move, string encoded, string san)
    {
        _legalMoveCalculatorMock
            .CalculateAllLegalMoves(Arg.Any<ChessBoard>(), GameColor.White)
            .Returns([move]);

        _encoderMock.EncodeMoves(Arg.Any<IEnumerable<Move>>()).Returns([encoded]);
        _encoderMock.EncodeSingleMove(move).Returns(encoded);

        Move[] legalMoves = [move];
        _sanCalculatorMock
            .CalculateSan(move, Arg.Is<IEnumerable<Move>>(moves => moves.SequenceEqual(legalMoves)))
            .Returns(san);
    }
}
