using System.Diagnostics.CodeAnalysis;
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
    public bool TryGetSeeker(UserId userId, [NotNullWhen(true)] out Seeker? seeker);

    List<(Seeker seeker1, Seeker seeker2)> CalculateMatches();
}
