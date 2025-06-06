using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.DTOs;

public record GameStateDto(
    string PlayerWhite,
    string PlayerBlack,
    string PlayerToMove,
    string Fen,
    IEnumerable<Move> Moves,
    IEnumerable<Move> LegalMoves
);
