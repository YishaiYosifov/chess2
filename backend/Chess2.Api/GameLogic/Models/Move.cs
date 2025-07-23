namespace Chess2.Api.GameLogic.Models;

public record Move
{
    public AlgebraicPoint From { get; init; }
    public AlgebraicPoint To { get; init; }
    public Piece Piece { get; init; }
    public IReadOnlyList<AlgebraicPoint> TriggerSquares { get; init; }
    public IReadOnlyList<AlgebraicPoint> CapturedSquares { get; init; }
    public IReadOnlyList<MoveSideEffect> SideEffects { get; init; }
    public SpecialMoveType SpecialMoveType { get; init; }
    public ForcedMovePriority ForcedPriority { get; init; }

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
