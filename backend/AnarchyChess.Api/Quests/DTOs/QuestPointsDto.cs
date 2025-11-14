using System.ComponentModel;
using AnarchyChess.Api.Profile.DTOs;

namespace AnarchyChess.Api.Quests.DTOs;

[DisplayName("UserQuestPoints")]
public record QuestPointsDto(MinimalProfile Profile, int QuestPoints);
