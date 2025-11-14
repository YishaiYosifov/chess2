using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameLogic;

public static class GameLogicConstants
{
    public const int BoardWidth = 10;
    public const int BoardHeight = 10;

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
