namespace Chess2.Api.Quests.DTOs;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.DTOs.QuestDto")]
public record QuestDto(string Description, int Progress, int Target);
