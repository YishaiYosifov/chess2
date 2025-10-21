using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Game.Models;

namespace Chess2.Api.QuestLogic.Models;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.Models.GameQuestSnapshot")]
public record GameQuestSnapshot(
    GameToken GameToken,
    GameColor PlayerColor,
    IReadOnlyList<Move> MoveHistory,
    GameResultData ResultData
);
