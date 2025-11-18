using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic;

public record QuestVariant(
    string Description,
    QuestDifficulty Difficulty,
    int Target,
    Func<IReadOnlyCollection<IQuestCondition>> Conditions,
    Func<IReadOnlyCollection<IQuestMetric>>? Progressors = null,
    bool ShouldResetOnFailure = false
)
{
    public QuestInstance CreateInstance(DateOnly? creationDate = null) =>
        new(
            Description,
            Difficulty,
            Target,
            creationDate ?? new DateOnly(),
            ShouldResetOnFailure,
            Conditions(),
            Progressors?.Invoke()
        );
}
