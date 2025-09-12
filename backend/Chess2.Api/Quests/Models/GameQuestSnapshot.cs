using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.Models.GameQuestSnapshot")]
public record GameQuestSnapshot(
    string GameToken,
    GameColor PlayerColor,
    IReadOnlyList<Move> MoveHistory,
    GameResultData? ResultData = null
);
