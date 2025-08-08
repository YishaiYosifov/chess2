using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.GameState")]
public record GameState(
    TimeControlSettings TimeControl,
    bool IsRated,
    GamePlayer WhitePlayer,
    GamePlayer BlackPlayer,
    ClockSnapshot Clocks,
    GameColor SideToMove,
    string InitialFen,
    IReadOnlyList<MoveSnapshot> MoveHistory,
    MoveOptions MoveOptions,
    DrawState DrawState,
    GameResultData? ResultData = null
);
