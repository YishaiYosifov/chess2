namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekCreatedBroadcastEvent")]
public record OpenSeekBroadcastEvent(Seeker Seeker, PoolKey Pool);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekEndedEvent")]
public record SeekEndedEvent(string? GameToken);
