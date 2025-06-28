using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameTests;

public class GameCoreTests
{
    private readonly IFenCalculator _fenCalculator = Substitute.For<IFenCalculator>();
    private readonly ILegalMoveCalculator _legalMoveCalculator =
        Substitute.For<ILegalMoveCalculator>();
    private readonly IMoveEncoder _encoder = Substitute.For<IMoveEncoder>();

    private readonly GameCore _gameCore;

    public GameCoreTests()
    {
        _gameCore = new(
            Substitute.For<ILogger<GameCore>>(),
            _fenCalculator,
            _legalMoveCalculator,
            _encoder
        );
    }

    [Fact]
    public void InitializeGame_sets_the_initial_state_correctly()
    {
        var w1 = new Move(new("e2"), new("e4"), PieceFactory.White());
        var w2 = new Move(new("g1"), new("f3"), PieceFactory.White());
        var b1 = new Move(new("e7"), new("e5"), PieceFactory.Black());
        var b2 = new Move(new("b8"), new("c6"), PieceFactory.Black());

        var allMoves = new[] { w1, w2, b1, b2 };

        var whiteEnc = new[] { "e2e4", "g1f3" };
        var blackEnc = new[] { "e7e5", "b8c6" };

        _legalMoveCalculator.CalculateAllLegalMoves(Arg.Any<ChessBoard>()).Returns(allMoves);
        _encoder
            .EncodeMoves(
                Arg.Is<IEnumerable<Move>>(m => m.All(x => x.Piece.Color == GameColor.White))
            )
            .Returns(whiteEnc);
        _encoder
            .EncodeMoves(
                Arg.Is<IEnumerable<Move>>(m => m.All(x => x.Piece.Color == GameColor.Black))
            )
            .Returns(blackEnc);
        _fenCalculator.CalculateFen(Arg.Any<ChessBoard>()).Returns("fen");

        _gameCore.InitializeGame();

        var whiteMoves = _gameCore.GetLegalMovesFor(GameColor.White);
        var blackMoves = _gameCore.GetLegalMovesFor(GameColor.Black);

        whiteMoves.Should().HaveCount(2);
        whiteMoves.Should().ContainKey((w1.From, w1.To));
        whiteMoves.Should().ContainKey((w2.From, w2.To));

        blackMoves.Should().HaveCount(2);
        blackMoves.Should().ContainKey((b1.From, b1.To));
        blackMoves.Should().ContainKey((b2.From, b2.To));

        _gameCore.GetEncodedLegalMovesFor(GameColor.White).Should().BeEquivalentTo(whiteEnc);
        _gameCore.GetEncodedLegalMovesFor(GameColor.Black).Should().BeEquivalentTo(blackEnc);
        _gameCore.Fen.Should().Be("fen");
    }

    [Fact]
    public void GetLegalMovesFor_returns_empty_when_uninitialized()
    {
        _gameCore.GetLegalMovesFor(GameColor.Black).Should().BeEmpty();
        _gameCore.GetLegalMovesFor(GameColor.White).Should().BeEmpty();
    }

    [Fact]
    public void MakeMove_returns_an_error_when_provided_an_invalid_move()
    {
        var from = new AlgebraicPoint("e2");
        var to = new AlgebraicPoint("e4");

        var result = _gameCore.MakeMove(from, to, GameColor.White);

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameErrors.MoveInvalid);
    }

    [Fact]
    public void MakeMove_moves_the_piece_and_updates_move_history_and_legal_moves()
    {
        var from = new AlgebraicPoint("e2");
        var to = new AlgebraicPoint("e4");
        var move = new Move(from, to, PieceFactory.White());
        var encoded = "e2e4";

        _legalMoveCalculator.CalculateAllLegalMoves(Arg.Any<ChessBoard>()).Returns([move]);
        _encoder.EncodeMoves(Arg.Any<IEnumerable<Move>>()).Returns([encoded]);
        _encoder.EncodeSingleMove(move).Returns(encoded);
        _fenCalculator.CalculateFen(Arg.Any<ChessBoard>()).Returns("updated-fen");

        _gameCore.InitializeGame();

        var result = _gameCore.MakeMove(from, to, GameColor.White);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(encoded);

        _gameCore.EncodedMoveHistory.Should().ContainSingle().Which.Should().Be(encoded);
        _gameCore.Fen.Should().Be("updated-fen");
        _gameCore.MoveNumber.Should().Be(1);
        _gameCore.SideToMove.Should().Be(GameColor.Black);
    }

    [Fact]
    public void SideToMove_should_alternate_depending_on_move_history()
    {
        var from = new AlgebraicPoint("e2");
        var to = new AlgebraicPoint("e4");
        var move = new Move(from, to, PieceFactory.White());

        _legalMoveCalculator.CalculateAllLegalMoves(Arg.Any<ChessBoard>()).Returns([move]);
        _encoder.EncodeMoves(Arg.Any<IEnumerable<Move>>()).Returns(["e2e4"]);
        _encoder.EncodeSingleMove(Arg.Any<Move>()).Returns("e2e4");
        _fenCalculator.CalculateFen(Arg.Any<ChessBoard>()).Returns("fen");

        _gameCore.InitializeGame();

        _gameCore.SideToMove.Should().Be(GameColor.White);
        _gameCore.MakeMove(from, to, GameColor.White);
        _gameCore.SideToMove.Should().Be(GameColor.Black);
    }
}
