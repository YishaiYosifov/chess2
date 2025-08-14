using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Users.Models;

namespace Chess2.Api.Lobby.Models;

public record OpenSeek(
    UserId UserId,
    string UserName,
    PoolKey Pool,
    TimeControl TimeControl,
    int? Rating
);
