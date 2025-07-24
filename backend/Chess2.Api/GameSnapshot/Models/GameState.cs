using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

public record GameState(
    TimeControlSettings TimeControl,
    bool IsRated,
    GamePlayer WhitePlayer,
    GamePlayer BlackPlayer,
    ClockSnapshot Clocks,
    GameColor SideToMove,
    string InitialFen,
    IReadOnlyList<MoveSnapshot> MoveHistory,
    IReadOnlyCollection<MovePath> LegalMoves,
    bool HasForcedMoves = false,
    GameResultData? ResultData = null
);
