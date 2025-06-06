using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public interface IPieceToLetter
{
    string ToLetter(PieceType piece);
}

public class PieceToLetter : IPieceToLetter
{
    private readonly Dictionary<PieceType, string> _pieceToLetterMap = new()
    {
        [PieceType.King] = "k",
        [PieceType.Queen] = "q",
        [PieceType.Pawn] = "p",
        [PieceType.Rook] = "r",
        [PieceType.Bishop] = "b",
        [PieceType.Horsey] = "h",
    };

    public string ToLetter(PieceType piece) => _pieceToLetterMap.GetValueOrDefault(piece, "?");
}
