using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestConditions.GreaterThanEqualCondition")]
public class GreaterThanEqualCondition(IQuestMetric inner, int greaterThanEqual) : IQuestCondition
{
    [Id(0)]
    private readonly IQuestMetric _inner = inner;

    [Id(1)]
    private readonly int _greaterThanEqual = greaterThanEqual;

    public bool Evaluate(GameQuestSnapshot snapshot) =>
        _inner.Evaluate(snapshot) >= _greaterThanEqual;
}
