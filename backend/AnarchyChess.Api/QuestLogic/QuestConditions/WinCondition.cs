using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestConditions.WinCondition")]
public class WinCondition : IQuestCondition
{
    public bool Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.PlayerColor.Match(
            whenWhite: snapshot.ResultData.Result == GameResult.WhiteWin,
            whenBlack: snapshot.ResultData.Result == GameResult.BlackWin
        );
}
