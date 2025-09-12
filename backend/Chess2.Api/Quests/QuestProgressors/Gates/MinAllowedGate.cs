using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Gates;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Gates.MinAllowedGate")]
public class MinAllowedGate(IQuestProgressor inner, int minProgress) : IQuestProgressor
{
    [Id(0)]
    private readonly IQuestProgressor _inner = inner;

    [Id(1)]
    private readonly int _minProgress = minProgress;

    public int EvaluateProgressMade(GameQuestSnapshot snapshot) =>
        _minProgress <= _inner.EvaluateProgressMade(snapshot) ? 1 : 0;
}
