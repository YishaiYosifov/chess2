using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Models;

public record GameState(
    TimeControlSettings TimeControl,
    bool IsRated,
    GamePlayer WhitePlayer,
    GamePlayer BlackPlayer,
    ClockDto Clocks,
    GameColor SideToMove,
    string Fen,
    IEnumerable<string> LegalMoves,
    IEnumerable<MoveSnapshot> MoveHistory,
    GameResultData? ResultData = null
);
