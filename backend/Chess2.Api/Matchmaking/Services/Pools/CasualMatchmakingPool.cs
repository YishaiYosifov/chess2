using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Users.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface ICasualMatchmakingPool : IMatchmakingPool;

public class CasualMatchmakingPool : ICasualMatchmakingPool
{
    private readonly Dictionary<string, Seeker> _seekers = [];

    public IEnumerable<Seeker> Seekers => _seekers.Values;
    public int SeekerCount => _seekers.Count;

    public bool AddSeek(Seeker seeker) => _seekers.TryAdd(seeker.UserId, seeker);

    public bool HasSeek(UserId userId) => _seekers.ContainsKey(userId);

    public bool RemoveSeek(UserId userId) => _seekers.Remove(userId);

    public List<(Seeker seeker1, Seeker seeker2)> CalculateMatches()
    {
        var matches = new List<(Seeker, Seeker)>();
        var unmatchedSeekers = new List<Seeker>(_seekers.Values);
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
