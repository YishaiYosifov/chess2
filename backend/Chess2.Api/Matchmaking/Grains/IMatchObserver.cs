using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Grains;

[Alias("Chess2.Api.Matchmaking.Grains.IMatchObserver")]
public interface IMatchObserver : IGrain
{
    [Alias("MatchFoundAsync")]
    Task MatchFoundAsync(string gameToken, PoolKey poolKey);
}
