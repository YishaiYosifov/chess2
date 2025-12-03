using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Matchmaking.Services.Pools;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IEnumerable<Seeker> Seekers { get; }

    void AddSeeker(Seeker seeker);
    bool RemoveSeeker(UserId userId);
    bool HasSeeker(UserId userId);
    bool TryGetSeeker(UserId userId, [NotNullWhen(true)] out Seeker? seeker);

    List<(Seeker Seeker1, Seeker Seeker2)> CalculateMatches();
}
