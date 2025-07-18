namespace Chess2.Api.GameLogic.Models;

public record Move
{
    public AlgebraicPoint From { get; }
    public AlgebraicPoint To { get; }
    public Piece Piece { get; }
    public IEnumerable<AlgebraicPoint> TriggerSquares { get; }
    public IEnumerable<AlgebraicPoint> CapturedSquares { get; }
    public IEnumerable<Move> SideEffects { get; }
    public SpecialMoveType SpecialMoveType { get; }

    public Move(
        AlgebraicPoint from,
        AlgebraicPoint to,
        Piece piece,
        IEnumerable<AlgebraicPoint>? triggerSquares = null,
        IEnumerable<AlgebraicPoint>? capturedSquares = null,
        IEnumerable<Move>? sideEffects = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None
    )
    {
        From = from;
        To = to;
        Piece = piece;
        TriggerSquares = triggerSquares ?? [];
        CapturedSquares = capturedSquares ?? [];
        SideEffects = sideEffects ?? [];
        SpecialMoveType = specialMoveType;
    }

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
