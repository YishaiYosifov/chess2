using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Lobby.Models;

public record OpenSeek(UserId UserId, string UserName, PoolKey Pool, int? Rating);
