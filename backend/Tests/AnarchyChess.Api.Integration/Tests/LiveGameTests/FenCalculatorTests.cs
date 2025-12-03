using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.TestInfrastructure;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class FenCalculatorTests : BaseIntegrationTest
{
    private readonly IFenCalculator _fenCalculator;

    public FenCalculatorTests(AnarchyChessWebApplicationFactory factory)
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

        var expectedFen = "rhnbqkbcar/pppdppdppp/10/10/9+/+9/10/10/PPPDPPDPPP/RHNBQKBCAR";
        result.Should().Be(expectedFen);
    }

    [Fact]
    public void CalculateFen_correctly_compresses_rows_with_different_piece_colors()
    {
        var pieces = new Dictionary<AlgebraicPoint, Piece>()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.King, GameColor.White),
            [new AlgebraicPoint("d1")] = new Piece(PieceType.Rook, GameColor.Black),
            [new AlgebraicPoint("g1")] = new Piece(PieceType.Queen, GameColor.White),
            [new AlgebraicPoint("c2")] = new Piece(PieceType.Pawn, GameColor.White),
        };
        var board = new ChessBoard(pieces);

        var result = _fenCalculator.CalculateFen(board);

        var expectedFen = "10/10/10/10/10/10/10/10/2P7/K2r2Q3";
        result.Should().Be(expectedFen);
    }

    [Fact]
    public void CalculateFen_correctly_places_a_piece_on_all_rows()
    {
        var pieces = new Dictionary<AlgebraicPoint, Piece>()
        {
            [new AlgebraicPoint("a1")] = new Piece(PieceType.King, GameColor.White),
            [new AlgebraicPoint("b2")] = new Piece(PieceType.Rook, GameColor.Black),
            [new AlgebraicPoint("c3")] = new Piece(PieceType.Queen, GameColor.White),
            [new AlgebraicPoint("d4")] = new Piece(PieceType.Pawn, GameColor.Black),
        };
        var board = new ChessBoard(pieces, height: 4, width: 4);

        var result = _fenCalculator.CalculateFen(board);

        var expectedFen = "3p/2Q1/1r2/K3";
        result.Should().Be(expectedFen);
    }
}
