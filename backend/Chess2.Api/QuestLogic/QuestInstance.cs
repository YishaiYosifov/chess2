using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestInstance")]
public class QuestInstance(
    string description,
    QuestDifficulty difficulty,
    int target,
    DateOnly creationDate,
    IReadOnlyCollection<IQuestCondition> conditions,
    IReadOnlyCollection<IQuestMetric>? metrics
)
{
    [Id(0)]
    private readonly IReadOnlyCollection<IQuestCondition> _conditions = conditions;

    [Id(1)]
    private readonly IReadOnlyCollection<IQuestMetric>? _metrics = metrics;

    [Id(2)]
    public string Description { get; } = description;

    [Id(3)]
    public QuestDifficulty Difficulty { get; } = difficulty;

    [Id(4)]
    public int Target { get; } = target;

    [Id(5)]
    public int Progress { get; private set; } = 0;

    [Id(6)]
    public DateOnly CreationDate { get; } = creationDate;

    public bool IsCompleted => Progress >= Target;

    public int ApplySnapshot(GameQuestSnapshot snapshot)
    {
        var progressMade = EvaluateProgressMade(snapshot);
        Progress = Math.Min(Progress + progressMade, Target);
        return progressMade;
    }

    private int EvaluateProgressMade(GameQuestSnapshot snapshot)
    {
        if (!_conditions.All(condition => condition.Evaluate(snapshot)))
            return 0;

        if (_metrics is null)
            return 1;

        return _metrics.Sum(metric => metric.Evaluate(snapshot));
    }
}
