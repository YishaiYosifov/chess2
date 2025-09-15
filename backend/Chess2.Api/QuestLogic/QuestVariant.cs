using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic;

public record QuestVariant(
    string Description,
    QuestDifficulty Difficulty,
    int Target,
    Func<IReadOnlyCollection<IQuestCondition>> Conditions,
    Func<IReadOnlyCollection<IQuestMetric>>? Progressors = null
)
{
    public QuestInstance CreateInstance(DateOnly? creationDate = null) =>
        new(
            Description,
            Difficulty,
            Target,
            creationDate ?? new DateOnly(),
            Conditions(),
            Progressors?.Invoke()
        );
}
