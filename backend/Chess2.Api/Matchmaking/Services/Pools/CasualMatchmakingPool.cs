using System.Diagnostics.CodeAnalysis;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Services.Pools.CasualMatchmakingPool")]
public class CasualMatchmakingPool : IMatchmakingPool
{
    [Id(0)]
    private readonly Dictionary<string, Seeker> _seekers = [];

    public IEnumerable<Seeker> Seekers => _seekers.Values;
    public int SeekerCount => _seekers.Count;

    public void AddSeeker(Seeker seeker) => _seekers[seeker.UserId] = seeker;

    public bool HasSeeker(UserId userId) => _seekers.ContainsKey(userId);

    public bool RemoveSeeker(UserId userId) => _seekers.Remove(userId);

    public bool TryGetSeeker(UserId userId, [NotNullWhen(true)] out Seeker? seeker) =>
        _seekers.TryGetValue(userId, out seeker);

    public List<(UserId User1, UserId User2)> CalculateMatches()
    {
        List<(UserId, UserId)> matches = [];
        List<Seeker> unmatchedSeekers = [.. _seekers.Values];
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

                matches.Add((seeker.UserId, candidate.UserId));
                matchedIds.Add(seeker.UserId);
                matchedIds.Add(candidate.UserId);
                break;
            }
        }

        return matches;
    }
}
