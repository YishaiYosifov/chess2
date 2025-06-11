using System.ComponentModel;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.DTOs;

[DisplayName("GameState")]
public record GameStateDto(
    GamePlayerDto PlayerWhite,
    GamePlayerDto PlayerBlack,
    GameColor CurrentPlayerColor,
    string Fen,
    IReadOnlyCollection<string> MoveHistory,
    IReadOnlyCollection<string> LegalMoves
);
