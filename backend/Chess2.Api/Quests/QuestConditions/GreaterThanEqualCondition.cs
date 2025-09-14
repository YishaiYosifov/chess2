using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestMetrics;

namespace Chess2.Api.Quests.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestConditions.GreaterThanEqualCondition")]
public class GreaterThanEqualCondition(IQuestMetric inner, int greaterThanEqual) : IQuestCondition
{
    [Id(0)]
    private readonly IQuestMetric _inner = inner;

    [Id(1)]
    private readonly int _greaterThanEqual = greaterThanEqual;

    public bool Evaluate(GameQuestSnapshot snapshot) =>
        _inner.Evaluate(snapshot) >= _greaterThanEqual;
}
