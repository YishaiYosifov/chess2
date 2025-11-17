using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.QuestLogic.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.Models.GameQuestSnapshot")]
public record GameQuestSnapshot(
    GameColor PlayerColor,
    IReadOnlyList<Move> MoveHistory,
    GameResultData ResultData,
    GameState FinalGameState
);
