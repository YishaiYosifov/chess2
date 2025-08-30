using Chess2.Api.Profile.Models;

namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.OpenSeekCreatedEvent")]
public record OpenSeekCreatedEvent(Seeker Seeker, PoolKey Pool);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.OpenSeekRemovedEvent")]
public record OpenSeekRemovedEvent(UserId UserId, PoolKey Pool);

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.PlayerSeekEndedEvent")]
public record PlayerSeekEndedEvent(string? GameToken);
