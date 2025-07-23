namespace Chess2.Api.GameLogic.Models;

public record Move
{
    public AlgebraicPoint From { get; }
    public AlgebraicPoint To { get; }
    public Piece Piece { get; }
    public IReadOnlyList<AlgebraicPoint> TriggerSquares { get; }
    public IReadOnlyList<AlgebraicPoint> CapturedSquares { get; }
    public IReadOnlyList<MoveSideEffect> SideEffects { get; }
    public SpecialMoveType SpecialMoveType { get; }
    public ForcedMovePriority ForcedPriority { get; set; }

    public Move(
        AlgebraicPoint from,
        AlgebraicPoint to,
        Piece piece,
        IEnumerable<AlgebraicPoint>? triggerSquares = null,
        IEnumerable<AlgebraicPoint>? capturedSquares = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None
    )
    {
        From = from;
        To = to;
        Piece = piece;
        TriggerSquares = triggerSquares?.ToList() ?? [];
        CapturedSquares = capturedSquares?.ToList() ?? [];
        SideEffects = sideEffects?.ToList() ?? [];
        SpecialMoveType = specialMoveType;
        ForcedPriority = forcedPriority;
    }

    public IEnumerable<(AlgebraicPoint From, AlgebraicPoint To)> Flatten()
    {
        if (SideEffects != null)
        {
            foreach (var side in SideEffects)
            {
                yield return (side.From, side.To);
            }
        }

        yield return (From, To);
    }
}
