using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.OpenSeekCreatedEvent")]
public record OpenSeekCreatedEvent(Seeker Seeker, PoolKey Pool);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.OpenSeekRemovedEvent")]
public record OpenSeekRemovedEvent(UserId UserId, PoolKey Pool);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.PlayerSeekEndedEvent")]
public record PlayerSeekEndedEvent(string? GameToken);
