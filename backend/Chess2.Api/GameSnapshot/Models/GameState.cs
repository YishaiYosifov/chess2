using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

public record GameState(
    TimeControlSettings TimeControl,
    bool IsRated,
    GamePlayer WhitePlayer,
    GamePlayer BlackPlayer,
    ClockSnapshot Clocks,
    GameColor SideToMove,
    string Fen,
    IReadOnlyCollection<MovePath> LegalMoves,
    IReadOnlyList<MoveSnapshot> MoveHistory,
    GameResultData? ResultData = null
);
