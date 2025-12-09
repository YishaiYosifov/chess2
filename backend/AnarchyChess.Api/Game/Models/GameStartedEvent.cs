using AnarchyChess.Api.Matchmaking.Models;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.GameStartedEvent")]
public record GameStartedEvent(OngoingGame Game, GameSource GameSource);
