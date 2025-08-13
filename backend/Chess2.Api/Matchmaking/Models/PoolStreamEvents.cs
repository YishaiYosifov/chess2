namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.OpenSeekCreatedEvent")]
public record OpenSeekCreatedEvent(SeekKey SeekKey, Seeker Seeker);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.OpenSeekRemovedEvent")]
public record OpenSeekRemovedEvent(SeekKey SeekKey);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.PlayerSeekEndedEvent")]
public record PlayerSeekEndedEvent(string? GameToken);
