using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.LiveGame.Models;

public readonly record struct MoveKey(
    AlgebraicPoint From,
    AlgebraicPoint To,
    PieceType? PromotesTo = null
)
{
    public override string ToString() =>
        PromotesTo is null ? $"{From}->{To}" : $"{From}->{To}={PromotesTo}";
}
