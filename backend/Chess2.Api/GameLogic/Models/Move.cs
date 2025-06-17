namespace Chess2.Api.GameLogic.Models;

public record Move(
    AlgebraicPoint From,
    AlgebraicPoint To,
    Piece Piece,
    IReadOnlyCollection<AlgebraicPoint>? Through = null,
    IReadOnlyCollection<AlgebraicPoint>? CapturedSquares = null,
    IReadOnlyCollection<Move>? SideEffects = null
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
