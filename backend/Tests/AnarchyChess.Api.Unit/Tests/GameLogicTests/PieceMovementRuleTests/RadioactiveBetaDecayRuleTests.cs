using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameLogic.PieceMovementRules;
using AnarchyChess.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class RadioactiveBetaDecayRuleTests
{
    private readonly Piece _movingPiece = PieceFactory.White();
    private readonly ChessBoard _board = new();

    [Fact]
    public void Evaluate_yields_no_move_when_any_decay_space_blocked()
    {
        AlgebraicPoint origin = new("d4");
        _board.PlacePiece(origin, _movingPiece);
        _board.PlacePiece(new AlgebraicPoint("e5"), PieceFactory.Black());

        Dictionary<Offset, PieceType> decays = new()
        {
            [new Offset(1, 1)] = PieceType.Pawn,
            [new Offset(2, 1)] = PieceType.Horsey,
        };
        RadioactiveBetaDecayRule rule = new(decays);

        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_spawns_pieces_at_specified_offsets()
    {
        AlgebraicPoint origin = new("c3");
        _board.PlacePiece(origin, _movingPiece);

        Dictionary<Offset, PieceType> decays = new()
        {
            [new Offset(1, 0)] = PieceType.Pawn,
            [new Offset(2, 0)] = PieceType.Horsey,
            [new Offset(3, 0)] = PieceType.Bishop,
        };
        RadioactiveBetaDecayRule rule = new(decays);

        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        var expectedMove = CreateExpectedDecayMove(origin, _movingPiece, decays);
        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_yields_no_move_when_board_edge_reached()
    {
        AlgebraicPoint origin = new("a1");
        _board.PlacePiece(origin, _movingPiece);

        var decays = new Dictionary<Offset, PieceType>
        {
            [new Offset(-1, 0)] = PieceType.Pawn, // outside board
        };
        RadioactiveBetaDecayRule rule = new(decays);

        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        result.Should().BeEmpty();
    }

    private static Move CreateExpectedDecayMove(
        AlgebraicPoint origin,
        Piece movingPiece,
        Dictionary<Offset, PieceType> decays
    )
    {
        List<PieceSpawn> spawns = [];
        foreach (var (offset, type) in decays)
        {
            spawns.Add(
                new PieceSpawn(Type: type, Color: movingPiece.Color, Position: origin + offset)
            );
        }

        return new Move(
            from: origin,
            to: origin,
            piece: movingPiece,
            captures: [new MoveCapture(movingPiece, origin)],
            pieceSpawns: spawns,
            specialMoveType: SpecialMoveType.RadioactiveBetaDecay
        );
    }
}
