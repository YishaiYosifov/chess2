using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameLogic.PieceMovementRules;
using Chess2.Api.TestInfrastructure.Factories;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.GameLogicTests.PieceMovementRuleTests;

public class RadioactiveBetaDecayRuleTests
{
    private readonly Piece _movingPiece = PieceFactory.White();
    private readonly ChessBoard _board = new();

    [Fact]
    public void Evaluate_yields_no_move_when_rank_not_empty()
    {
        AlgebraicPoint origin = new("d4");
        _board.PlacePiece(origin, _movingPiece);
        _board.PlacePiece(new AlgebraicPoint("b4"), PieceFactory.Black());

        RadioactiveBetaDecayRule rule = new(PieceType.Pawn, PieceType.Horsey);

        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_spawns_pieces_from_opposite_edge_left_of_center()
    {
        AlgebraicPoint origin = new("c3");
        _board.PlacePiece(origin, _movingPiece);

        PieceType[] decayInto = [PieceType.Pawn, PieceType.Horsey, PieceType.Bishop];
        RadioactiveBetaDecayRule rule = new(decayInto);

        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        var expectedMove = CreateExpectedDecayMove(
            origin,
            _movingPiece,
            decayInto,
            startX: 9,
            offsetX: -1
        );
        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void Evaluate_spawns_pieces_from_opposite_edge_right_of_center()
    {
        AlgebraicPoint origin = new("f4");
        _board.PlacePiece(origin, _movingPiece);

        PieceType[] decayInto = [PieceType.Rook, PieceType.Horsey];
        RadioactiveBetaDecayRule rule = new(decayInto);

        var result = rule.Evaluate(_board, origin, _movingPiece).ToList();

        var expectedMove = CreateExpectedDecayMove(
            origin,
            _movingPiece,
            decayInto,
            startX: 0,
            offsetX: 1
        );
        result.Should().BeEquivalentTo([expectedMove]);
    }

    private static Move CreateExpectedDecayMove(
        AlgebraicPoint origin,
        Piece movingPiece,
        PieceType[] decayInto,
        int startX,
        int offsetX
    )
    {
        List<PieceSpawn> spawns = [];
        int x = startX;

        foreach (var type in decayInto)
        {
            spawns.Add(
                new PieceSpawn(
                    Type: type,
                    Color: movingPiece.Color,
                    Position: new AlgebraicPoint(x, origin.Y)
                )
            );
            x += offsetX;
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
