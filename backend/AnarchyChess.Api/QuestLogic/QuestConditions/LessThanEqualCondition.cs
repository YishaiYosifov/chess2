using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestConditions.LessThanEqualCondition")]
public class LessThanEqualCondition(IQuestMetric inner, int lessThanEqual) : IQuestCondition
{
    [Id(0)]
    private readonly IQuestMetric _inner = inner;

    [Id(1)]
    private readonly int _lessThanEqual = lessThanEqual;

    public bool Evaluate(GameQuestSnapshot snapshot) => _inner.Evaluate(snapshot) <= _lessThanEqual;
}
