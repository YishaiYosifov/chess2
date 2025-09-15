using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestMetrics;

namespace Chess2.Api.QuestLogic;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestVariant")]
public record QuestVariant(
    string Description,
    QuestDifficulty Difficulty,
    int Target,
    Func<IReadOnlyCollection<IQuestCondition>> Conditions,
    Func<IReadOnlyCollection<IQuestMetric>>? Progressors = null
)
{
    public QuestInstance CreateInstance() =>
        new(Description, Difficulty, Target, Conditions(), Progressors?.Invoke());
}
