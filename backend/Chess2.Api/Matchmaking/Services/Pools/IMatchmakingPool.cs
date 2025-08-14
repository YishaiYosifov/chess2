using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Users.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IEnumerable<Seeker> Seekers { get; }

    void AddSeek(Seeker seeker);
    bool RemoveSeek(UserId userId);
    bool HasSeeker(UserId userId);
    List<(Seeker seeker1, Seeker seeker2)> CalculateMatches();
}
