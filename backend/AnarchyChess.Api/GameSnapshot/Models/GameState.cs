using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Matchmaking.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.GameState")]
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
