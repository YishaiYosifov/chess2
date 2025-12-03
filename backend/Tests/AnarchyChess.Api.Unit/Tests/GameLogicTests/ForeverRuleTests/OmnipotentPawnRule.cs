using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.ForeverRules;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.GameLogicTests.ForeverRuleTests;

public class OmnipotentPawnRuleTests
{
    private readonly ChessBoard _board = new();

    [Fact]
    public void GetBehaviours_yields_no_move_when_no_previous_move()
    {
        OmnipotentPawnRule rule = new();
        var result = rule.GetBehaviours(_board, GameColor.White).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetBehaviours_yields_no_move_when_last_move_not_to_spawn_position()
    {
        Move lastMove = new(
            from: new AlgebraicPoint("a1"),
            to: new AlgebraicPoint("b2"),
            piece: PieceFactory.Black()
        );
        _board.PlacePiece(lastMove.From, lastMove.Piece);
        _board.PlayMove(lastMove);

        OmnipotentPawnRule rule = new();
        var result = rule.GetBehaviours(_board, GameColor.White).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetBehaviours_yields_no_move_when_last_move_did_not_capture_player_piece()
    {
        AlgebraicPoint spawnPosition = new("h3");
        Move lastMove = new(
            from: new AlgebraicPoint("g2"),
            to: spawnPosition,
            piece: PieceFactory.Black(),
            captures: [new MoveCapture(PieceFactory.Black(), spawnPosition)]
        );
        _board.PlacePiece(lastMove.From, lastMove.Piece);
        _board.PlayMove(lastMove);

        OmnipotentPawnRule rule = new();
        var result = rule.GetBehaviours(_board, GameColor.White).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetBehaviours_spawns_pawn_when_conditions_met_for_white()
    {
        AlgebraicPoint spawnPosition = new("h3");
        Move lastMove = new(
            from: new AlgebraicPoint("g2"),
            to: spawnPosition,
            piece: PieceFactory.Black(),
            captures: [new MoveCapture(PieceFactory.White(), spawnPosition)]
        );
        _board.PlacePiece(lastMove.From, lastMove.Piece);
        _board.PlayMove(lastMove);

        OmnipotentPawnRule rule = new();
        var result = rule.GetBehaviours(_board, GameColor.White).ToList();

        Move expectedMove = new(
            from: spawnPosition,
            to: spawnPosition,
            piece: lastMove.Piece,
            captures: [new MoveCapture(spawnPosition, _board)],
            pieceSpawns: [new PieceSpawn(PieceType.Pawn, Color: GameColor.White, spawnPosition)],
            specialMoveType: SpecialMoveType.OmnipotentPawnSpawn
        );

        result.Should().BeEquivalentTo([expectedMove]);
    }

    [Fact]
    public void GetBehaviours_spawns_pawn_when_conditions_met_for_black()
    {
        AlgebraicPoint spawnPosition = new("h8");
        Move lastMove = new(
            from: new AlgebraicPoint("g7"),
            to: spawnPosition,
            piece: PieceFactory.White(),
            captures: [new MoveCapture(PieceFactory.Black(), spawnPosition)]
        );
        _board.PlacePiece(lastMove.From, lastMove.Piece);
        _board.PlayMove(lastMove);

        OmnipotentPawnRule rule = new();
        var result = rule.GetBehaviours(_board, GameColor.Black).ToList();

        Move expectedMove = new(
            from: spawnPosition,
            to: spawnPosition,
            piece: lastMove.Piece,
            captures: [new MoveCapture(spawnPosition, _board)],
            pieceSpawns: [new PieceSpawn(PieceType.Pawn, Color: GameColor.Black, spawnPosition)],
            specialMoveType: SpecialMoveType.OmnipotentPawnSpawn
        );

        result.Should().BeEquivalentTo([expectedMove]);
    }
}
