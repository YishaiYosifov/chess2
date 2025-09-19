using System.Text.Json.Serialization;

namespace Chess2.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameLogic.Models.Move")]
[method: JsonConstructor]
public record Move(
    AlgebraicPoint From,
    AlgebraicPoint To,
    Piece Piece,
    IReadOnlyCollection<AlgebraicPoint> TriggerSquares,
    IReadOnlyCollection<AlgebraicPoint> IntermediateSquares,
    IReadOnlyCollection<MoveCapture> Captures,
    IReadOnlyCollection<MoveSideEffect> SideEffects,
    SpecialMoveType SpecialMoveType,
    ForcedMovePriority ForcedPriority,
    PieceType? PromotesTo
)
{
    public Move(
        AlgebraicPoint from,
        AlgebraicPoint to,
        Piece piece,
        IEnumerable<AlgebraicPoint>? triggerSquares = null,
        IEnumerable<AlgebraicPoint>? intermediateSquares = null,
        IEnumerable<MoveCapture>? captures = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null
    )
        : this(
            From: from,
            To: to,
            Piece: piece,
            TriggerSquares: triggerSquares?.ToList() ?? [],
            IntermediateSquares: intermediateSquares?.ToList() ?? [],
            Captures: captures?.ToList() ?? [],
            SideEffects: sideEffects?.ToList() ?? [],
            SpecialMoveType: specialMoveType,
            ForcedPriority: forcedPriority,
            PromotesTo: promotesTo
        ) { }

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
