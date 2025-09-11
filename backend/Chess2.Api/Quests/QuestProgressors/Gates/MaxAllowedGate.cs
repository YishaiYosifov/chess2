using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors.Gates;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Gates.MaxAllowedGate")]
public class MaxAllowedGate(IQuestProgressor inner, int maxProgress) : IQuestProgressor
{
    [Id(0)]
    private readonly IQuestProgressor _inner = inner;

    [Id(1)]
    private readonly int _maxProgress = maxProgress;

    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor) =>
        _maxProgress >= _inner.EvaluateProgressMade(snapshot, playerColor) ? 1 : 0;
}
