using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IEnumerable<string> Seekers { get; }

    bool TryAddSeek(Seeker seeker);
    bool RemoveSeek(string userId);
    bool HasSeek(string userId);
    List<(Seeker seeker1, Seeker seeker2)> CalculateMatches();
}
