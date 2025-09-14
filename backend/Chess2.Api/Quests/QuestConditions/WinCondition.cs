using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestConditions.WinCondition")]
public class WinCondition : IQuestCondition
{
    public bool Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.PlayerColor.Match(
            whenWhite: snapshot.ResultData.Result == GameResult.WhiteWin,
            whenBlack: snapshot.ResultData.Result == GameResult.BlackWin
        );
}
