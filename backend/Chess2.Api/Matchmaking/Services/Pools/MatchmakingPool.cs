using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IReadOnlyList<SeekInfo> Seekers { get; }

    void AddSeek(string userId, int rating);
    bool RemoveSeek(string userId);
    List<(string userId1, string userId2)> CalculateMatches();
}

public abstract class MatchmakingPool : IMatchmakingPool
{
    protected readonly Dictionary<string, SeekInfo> _seekers = [];

    public IReadOnlyList<SeekInfo> Seekers => _seekers.Values.ToList().AsReadOnly();
    public int SeekerCount => _seekers.Count;

    public virtual void AddSeek(string userId, int rating)
    {
        var seekInfo = new SeekInfo(userId, rating);
        _seekers[userId] = seekInfo;
    }

    public virtual bool RemoveSeek(string userId) => _seekers.Remove(userId);

    public abstract List<(string userId1, string userId2)> CalculateMatches();
}
