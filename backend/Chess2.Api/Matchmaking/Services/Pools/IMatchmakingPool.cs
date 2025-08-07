using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IEnumerable<string> Seekers { get; }

    bool TryAddSeek(Seek seek);
    bool RemoveSeek(string userId);
    bool HasSeek(string userId);
    List<(Seek seek1, Seek seek2)> CalculateMatches();
}
