using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameLogic;

public static class GameLogicConstants
{
    public static readonly IReadOnlySet<PieceType> PawnLikePieces = new HashSet<PieceType>(
        [PieceType.Pawn, PieceType.UnderagePawn, PieceType.SterilePawn]
    );

    public static readonly IReadOnlyCollection<PieceType> PromotablePieces =
    [
        PieceType.Queen,
        PieceType.Rook,
        PieceType.Bishop,
        PieceType.Horsey,
        PieceType.Knook,
        PieceType.Antiqueen,
        PieceType.Checker,
    ];
}
