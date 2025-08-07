using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface ICasualMatchmakingPool : IMatchmakingPool;

public class CasualMatchmakingPool : ICasualMatchmakingPool
{
    private readonly Dictionary<string, Seek> _seekers = [];

    public IEnumerable<string> Seekers => _seekers.Keys;
    public int SeekerCount => _seekers.Count;

    public bool TryAddSeek(Seek seek) => _seekers.TryAdd(seek.UserId, seek);

    public bool HasSeek(string userId) => _seekers.ContainsKey(userId);

    public bool RemoveSeek(string userId) => _seekers.Remove(userId);

    public List<(Seek seek1, Seek seek2)> CalculateMatches()
    {
        var matches = new List<(Seek, Seek)>();
        var unmatchedSeekers = new List<Seek>(_seekers.Values);
        var matchedIds = new HashSet<string>();

        for (int i = 0; i < unmatchedSeekers.Count; i++)
        {
            var seeker = unmatchedSeekers[i];
            if (matchedIds.Contains(seeker.UserId))
                continue;

            for (int j = i + 1; j < unmatchedSeekers.Count; j++)
            {
                var candidate = unmatchedSeekers[j];
                if (matchedIds.Contains(candidate.UserId))
                    continue;

                if (!seeker.IsCompatibleWith(candidate) || !candidate.IsCompatibleWith(seeker))
                    continue;

                matches.Add((seeker, candidate));
                matchedIds.Add(seeker.UserId);
                matchedIds.Add(candidate.UserId);
                break;
            }
        }

        foreach (var id in matchedIds)
        {
            _seekers.Remove(id);
        }

        return matches;
    }
}
