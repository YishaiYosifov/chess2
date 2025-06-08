namespace Chess2.Api.GameLogic.Models;

public record Move(
    Point From,
    Point To,
    Piece Piece,
    IEnumerable<Point>? Through = null,
    IEnumerable<Point>? CapturedSquares = null,
    IEnumerable<Move>? SideEffects = null
);
