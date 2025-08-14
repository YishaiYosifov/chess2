using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Users.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IEnumerable<Seeker> Seekers { get; }

    void AddSeeker(Seeker seeker);
    bool RemoveSeeker(UserId userId);
    bool HasSeeker(UserId userId);
    Seeker? GetSeeker(UserId userId);

    List<(Seeker seeker1, Seeker seeker2)> CalculateMatches();
}
