namespace Chess2.Api.Matchmaking.Services.Pools;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IReadOnlyList<string> Seekers { get; }

    bool RemoveSeek(string userId);
    List<(string userId1, string userId2)> CalculateMatches();
}
