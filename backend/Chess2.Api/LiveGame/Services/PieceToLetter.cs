using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.LiveGame.Services;

public interface IPieceToLetter
{
    string GetLetter(PieceType piece);
}

public class PieceToLetter : IPieceToLetter
{
    private readonly Dictionary<PieceType, string> _pieceToLetterMap = new()
    {
        [PieceType.King] = "k",
        [PieceType.Queen] = "q",
        [PieceType.Pawn] = "p",
        [PieceType.ChildPawn] = "d",
        [PieceType.Rook] = "r",
        [PieceType.Bishop] = "b",
        [PieceType.Horsey] = "h",
        [PieceType.Knook] = "n",
    };

    public string GetLetter(PieceType piece) => _pieceToLetterMap.GetValueOrDefault(piece, "?");
}
