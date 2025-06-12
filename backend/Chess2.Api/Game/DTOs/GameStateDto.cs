using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using System.ComponentModel;

namespace Chess2.Api.Game.DTOs;

[DisplayName("GameState")]
public record GameStateDto(
    GamePlayer PlayerWhite,
    GamePlayer PlayerBlack,
    GameColor CurrentPlayerColor,
    string Fen,
    IReadOnlyCollection<string> MoveHistory,
    IReadOnlyCollection<string> LegalMoves
);
