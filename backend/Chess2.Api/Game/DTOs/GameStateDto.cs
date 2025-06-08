using System.ComponentModel;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.DTOs;

[DisplayName("GameState")]
public record GameStateDto(
    GamePlayerDto PlayerWhite,
    GamePlayerDto PlayerBlack,
    GameColor PlayerToMove,
    string Fen,
    IReadOnlyCollection<string> FenHistory,
    string LegalMoves
);
