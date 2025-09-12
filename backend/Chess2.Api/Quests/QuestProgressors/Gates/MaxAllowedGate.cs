using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Gates;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Gates.MaxAllowedGate")]
public class MaxAllowedGate(IQuestProgressor inner, int maxProgress) : IQuestProgressor
{
    [Id(0)]
    private readonly IQuestProgressor _inner = inner;

    [Id(1)]
    private readonly int _maxProgress = maxProgress;

    public int EvaluateProgressMade(GameQuestSnapshot snapshot) =>
        _maxProgress >= _inner.EvaluateProgressMade(snapshot) ? 1 : 0;
}
