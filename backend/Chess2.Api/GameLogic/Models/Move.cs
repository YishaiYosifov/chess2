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

    public Move(
        AlgebraicPoint from,
        AlgebraicPoint to,
        Piece piece,
        IEnumerable<AlgebraicPoint>? triggerSquares = null,
        IEnumerable<AlgebraicPoint>? capturedSquares = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None
    )
    {
        From = from;
        To = to;
        Piece = piece;
        TriggerSquares = triggerSquares?.ToList() ?? [];
        CapturedSquares = capturedSquares?.ToList() ?? [];
        SideEffects = sideEffects?.ToList() ?? [];
        SpecialMoveType = specialMoveType;
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
