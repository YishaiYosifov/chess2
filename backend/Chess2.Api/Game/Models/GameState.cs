using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Models;

public record GameState(
    GamePlayer WhitePlayer,
    GamePlayer BlackPlayer,
    GameColor SideToMove,
    string Fen,
    IReadOnlyCollection<string> MoveHistory,
    IReadOnlyCollection<string> LegalMoves,
    TimeControlSettings TimeControl
);
