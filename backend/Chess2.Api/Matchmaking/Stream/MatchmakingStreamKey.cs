using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Users.Models;

namespace Chess2.Api.Matchmaking.Stream;

public static class MatchmakingStreamKey
{
    public static string MatchedStream(UserId userId, PoolKey pool) =>
        $"{pool.ToGrainKey()}:{userId}";
}
