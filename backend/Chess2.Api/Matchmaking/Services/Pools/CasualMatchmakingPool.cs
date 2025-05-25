
namespace Chess2.Api.Matchmaking.Services.Pools;

public interface ICasualMatchmakingPool : IMatchmakingPool
{
    void AddSeek(string userId);
}

public class CasualMatchmakingPool : ICasualMatchmakingPool
{
    private readonly HashSet<string> _seekers = [];

    public int SeekerCount => _seekers.Count;
    public IReadOnlyList<string> Seekers => _seekers.ToList().AsReadOnly();


    public void AddSeek(string userId) => _seekers.Add(userId);

    public bool RemoveSeek(string userId) => _seekers.Remove(userId);

    public List<(string userId1, string userId2)> CalculateMatches()
    {
        var matches = new List<(string, string)>();
        var seekers = _seekers.ToArray();
        string? unmatchedUser = null;
        for (int i = 0; i < seekers.Length; i += 2)
        {
            if (i + 1 >= seekers.Length)
            {
                // Odd number of seekers, last one remains unmatched
                unmatchedUser = seekers[i];
                break;
            }

            var seeker1 = seekers[i];
            var seeker2 = seekers[i + 1];

            matches.Add((seeker1, seeker2));
        }

        _seekers.Clear();
        if (unmatchedUser is not null)
            _seekers.Add(unmatchedUser);

        return matches;
    }
}
