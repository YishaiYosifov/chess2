using Chess2.Api.Quests.QuestProgressors;

namespace Chess2.Api.Quests.Models;

public record QuestVariant(
    IQuestProgressor Quest,
    string Description,
    int Target,
    QuestDifficulty Difficulty
);
