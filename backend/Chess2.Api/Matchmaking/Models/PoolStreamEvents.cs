namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekCreatedBroadcastEvent")]
public record OpenSeekBroadcastEvent(SeekKey SeekKey, Seeker Seeker);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekEndedBroadcastEvent")]
public record OpenSeekEndedBroadcastEvent(SeekKey SeekKey);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekEndedEvent")]
public record SeekEndedEvent(string? GameToken);
