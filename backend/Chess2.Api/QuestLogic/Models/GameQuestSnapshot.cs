using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.QuestLogic.Models;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.Models.GameQuestSnapshot")]
public record GameQuestSnapshot(
    GameColor PlayerColor,
    IReadOnlyList<Move> MoveHistory,
    GameResultData ResultData
);
