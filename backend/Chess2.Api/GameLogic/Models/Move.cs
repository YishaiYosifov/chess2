namespace Chess2.Api.GameLogic.Models;

public record Move(
    Point From,
    Point To,
    Piece Piece,
    IEnumerable<Point>? Through = null,
    IEnumerable<Point>? CapturedSquares = null,
    IEnumerable<Move>? SideEffects = null
)
{
    public IEnumerable<Move> Flatten()
    {
        if (SideEffects != null)
        {
            foreach (var side in SideEffects)
            {
                foreach (var nested in side.Flatten())
                    yield return nested;
            }
        }

        yield return this;
    }
}
