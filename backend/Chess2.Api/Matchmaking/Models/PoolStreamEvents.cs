namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekCreatedBroadcastEvent")]
public record SeekCreatedBroadcastEvent(Seeker Seeker, PoolKey Pool);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekInvalidatedEvent")]
public record SeekInvalidatedEvent(bool IsMatched);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekMatchedEvent")]
public record SeekMatchedEvent(string GameToken);
