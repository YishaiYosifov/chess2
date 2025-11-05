using System.Diagnostics.CodeAnalysis;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IEnumerable<Seeker> Seekers { get; }

    void AddSeeker(Seeker seeker);
    bool RemoveSeeker(UserId userId);
    bool HasSeeker(UserId userId);
    public bool TryGetSeeker(UserId userId, [NotNullWhen(true)] out Seeker? seeker);

    List<(UserId User1, UserId User2)> CalculateMatches();
}
