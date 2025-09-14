using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestConditions;
using Chess2.Api.Quests.QuestMetrics;

namespace Chess2.Api.Quests;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestEvaluator")]
public class QuestEvaluator(
    IReadOnlyCollection<IQuestCondition> conditions,
    IReadOnlyCollection<IQuestMetric>? metrics
)
{
    [Id(0)]
    private readonly IReadOnlyCollection<IQuestCondition> _conditions = conditions;

    [Id(1)]
    private readonly IReadOnlyCollection<IQuestMetric>? _metrics = metrics;

    public int Evaluate(GameQuestSnapshot snapshot)
    {
        if (!_conditions.All(condition => condition.Evaluate(snapshot)))
            return 0;

        if (_metrics is null)
            return 1;

        return _metrics.Sum(metric => metric.Evaluate(snapshot));
    }
}
