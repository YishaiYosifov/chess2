using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Profile.DTOs;

namespace AnarchyChess.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Matchmaking.Models.OngoingGame")]
public record OngoingGame(GameToken GameToken, PoolKey Pool, MinimalProfile Opponent);
