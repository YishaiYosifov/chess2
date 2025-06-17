namespace Chess2.Api.GameLogic.Models;

public record Move(
    AlgebraicPoint From,
    AlgebraicPoint To,
    Piece Piece,
    IEnumerable<AlgebraicPoint>? Through = null,
    IEnumerable<AlgebraicPoint>? CapturedSquares = null,
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
