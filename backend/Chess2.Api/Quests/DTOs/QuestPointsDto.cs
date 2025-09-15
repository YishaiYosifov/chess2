using System.ComponentModel;
using Chess2.Api.Profile.DTOs;

namespace Chess2.Api.Quests.DTOs;

[DisplayName("UserQuestPoints")]
public record QuestPointsDto(MinimalProfile Profile, int QuestPoints);
