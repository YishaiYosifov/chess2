using Chess2.Api.Profile.DTOs;
using System.ComponentModel;

namespace Chess2.Api.Quests.DTOs;

[DisplayName("UserQuestPoints")]
public record QuestPointsDto(MinimalProfile Profile, int QuestPoints);
