using Chess2.Api.Quests.QuestProgressors;

namespace Chess2.Api.Quests.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.Models.QuestVariant")]
public record QuestVariant(
    IQuestProgressor Progressor,
    string Description,
    int Target,
    QuestDifficulty Difficulty
);
