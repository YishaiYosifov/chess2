using Chess2.Api.Quests.QuestConditions;
using Chess2.Api.Quests.QuestMetrics;

namespace Chess2.Api.Quests.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.Models.QuestVariant")]
public record QuestVariant(
    string Description,
    QuestDifficulty Difficulty,
    int Target,
    IReadOnlyCollection<IQuestCondition> Conditions,
    IReadOnlyCollection<IQuestMetric>? Progressors = null
);
