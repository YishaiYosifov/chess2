using System.ComponentModel;
using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.Quests.DTOs;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Quests.DTOs.QuestDto")]
[DisplayName("Quest")]
public record QuestDto(
    QuestDifficulty Difficulty,
    string Description,
    int Target,
    int Progress,
    bool CanReplace,
    bool RewardCollected,
    int Streak
);
