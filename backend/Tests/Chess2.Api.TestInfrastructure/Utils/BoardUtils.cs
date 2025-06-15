using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.Factories;

namespace Chess2.Api.TestInfrastructure.Utils;

public static class BoardUtils
{
    public static ChessBoard CreateBoardWithPieces(
        Point from,
        Piece? piece = null,
        IEnumerable<Point>? blockingPieces = null
    )
    {
        var board = new ChessBoard([]);
        piece ??= PieceFactory.White();
        board.PlacePiece(from, piece);

        foreach (var p in blockingPieces ?? [])
            board.PlacePiece(p, PieceFactory.Black());

        return board;
    }
}
