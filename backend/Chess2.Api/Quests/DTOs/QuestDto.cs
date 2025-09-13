using System.ComponentModel;
using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.DTOs;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.DTOs.QuestDto")]
[DisplayName("Quest")]
public record QuestDto(
    QuestDifficulty Difficulty,
    string Description,
    int Progress,
    int Target,
    int Streak
);
