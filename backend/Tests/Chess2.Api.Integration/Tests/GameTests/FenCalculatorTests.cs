using Chess2.Api.Game;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.GameTests;

public class FenCalculatorTests : BaseIntegrationTest
{
    private readonly IFenCalculator _fenCalculator;

    public FenCalculatorTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _fenCalculator = Scope.ServiceProvider.GetRequiredService<IFenCalculator>();
    }

    [Fact]
    public void CalculateFen_returns_all_numbers_for_empty_board()
    {
        var board = new ChessBoard([], height: 10, width: 10);

        var result = _fenCalculator.CalculateFen(board);

        result.Should().Be("10/10/10/10/10/10/10/10/10/10");
    }

    [Fact]
    public void CalculateFen_returns_the_correct_fen_for_the_starting_position()
    {
        var board = new ChessBoard(GameConstants.StartingPosition);

        var result = _fenCalculator.CalculateFen(board);

        var expectedFen = "RH2QKB1HR/PP1PPPP1PP/10/10/10/10/10/10/pp1pppp1pp/rh2qkb1hr";
        result.Should().Be(expectedFen);
    }

    [Fact]
    public void CalculateFen_correctly_compresses_rows_with_different_piece_colors()
    {
        var pieces = new Dictionary<Point, Piece>()
        {
            [new Point(0, 0)] = new Piece(PieceType.King, GameColor.White),
            [new Point(3, 0)] = new Piece(PieceType.Rook, GameColor.Black),
            [new Point(6, 0)] = new Piece(PieceType.Queen, GameColor.White),
            [new Point(2, 1)] = new Piece(PieceType.Pawn, GameColor.White),
        };
        var board = new ChessBoard(pieces);

        var result = _fenCalculator.CalculateFen(board);

        var expectedFen = "10/10/10/10/10/10/10/10/2p7/k2R2q3";
        result.Should().Be(expectedFen);
    }

    [Fact]
    public void CalculateFen_correctly_places_a_piece_on_all_rows()
    {
        var pieces = new Dictionary<Point, Piece>()
        {
            [new Point(0, 0)] = new Piece(PieceType.King, GameColor.White),
            [new Point(1, 1)] = new Piece(PieceType.Rook, GameColor.Black),
            [new Point(2, 2)] = new Piece(PieceType.Queen, GameColor.White),
            [new Point(3, 3)] = new Piece(PieceType.Pawn, GameColor.Black),
        };
        var board = new ChessBoard(pieces, height: 4, width: 4);

        var result = _fenCalculator.CalculateFen(board);

        var expectedFen = "3P/2q1/1R2/k3";
        result.Should().Be(expectedFen);
    }
}
