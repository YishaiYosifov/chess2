using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Conditions;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Conditions.WinCondition")]
public class WinCondition(IQuestProgressor? inner = null) : IQuestProgressor
{
    [Id(0)]
    private readonly IQuestProgressor? _inner = inner;

    public int EvaluateProgressMade(GameQuestSnapshot snapshot)
    {
        if (snapshot.ResultData is null)
            return 0;

        bool didWin = snapshot.PlayerColor.Match(
            whenWhite: snapshot.ResultData.Result == GameResult.WhiteWin,
            whenBlack: snapshot.ResultData.Result == GameResult.BlackWin
        );
        if (!didWin)
            return 0;

        return _inner is not null ? _inner.EvaluateProgressMade(snapshot) : 1;
    }
}
