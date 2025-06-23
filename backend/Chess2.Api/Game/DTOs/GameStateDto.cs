using System.ComponentModel;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.DTOs;

[DisplayName("GameState")]
public record GameStateDto(
    GamePlayer PlayerWhite,
    GamePlayer PlayerBlack,
    GameColor SideToMove,
    string Fen,
    IReadOnlyCollection<string> MoveHistory,
    IReadOnlyCollection<string> LegalMoves,
    TimeControlSettings TimeControl
);
