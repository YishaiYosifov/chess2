using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Matchmaking.Models;

public record PoolKeyRequest(PoolType PoolType, TimeControlSettingsRequest TimeControl);
