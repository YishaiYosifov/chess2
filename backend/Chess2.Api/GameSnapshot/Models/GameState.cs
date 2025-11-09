using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.GameState")]
public record GameState(
    int Revision,
    GameSource GameSource,
    PoolKey Pool,
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
