using System.Diagnostics.CodeAnalysis;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Matchmaking.Services.Pools;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Services.Pools.RatedPoolMember")]
public class RatedPoolMember(RatedSeeker seek, int wavesMissed = 0)
{
    [Id(0)]
    public RatedSeeker Seek { get; } = seek;

    [Id(1)]
    public int WavesMissed { get; set; } = wavesMissed;
}

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Services.Pools.RatedMatchmakingPool")]
public class RatedMatchmakingPool : IMatchmakingPool
{
    [Id(0)]
    private readonly Dictionary<string, RatedPoolMember> _seekers = [];

    public IEnumerable<Seeker> Seekers => _seekers.Values.Select(x => x.Seek);
    public int SeekerCount => _seekers.Count;

    public void AddSeeker(Seeker seeker)
    {
        if (seeker is not RatedSeeker ratedSeek)
            throw new ArgumentException($"Seeker must be a {nameof(RatedSeeker)}", nameof(seeker));

        var seekInfo = new RatedPoolMember(ratedSeek);
        _seekers[seeker.UserId] = seekInfo;
    }

    public bool HasSeeker(UserId userId) => _seekers.ContainsKey(userId);

    public bool RemoveSeeker(UserId userId) => _seekers.Remove(userId);

    public bool TryGetSeeker(UserId userId, [NotNullWhen(true)] out Seeker? seeker)
    {
        var poolMember = _seekers.GetValueOrDefault(userId);
        seeker = poolMember?.Seek;
        return seeker is not null;
    }

    public List<(UserId User1, UserId User2)> CalculateMatches()
    {
        List<(UserId, UserId)> matches = [];
        HashSet<string> alreadyMatched = [];

        var seekersByRating = _seekers.Values.OrderBy(x => x.Seek.Rating.Value).ToList();

        foreach (var seeker in _seekers.Values)
        {
            if (alreadyMatched.Contains(seeker.Seek.UserId))
                continue;

            var startIdx = seeker.Seek.Rating.MinRating is null
                ? 0
                : BinarySearchLow(seekersByRating, seeker.Seek.Rating.MinRating.Value);
            var bestMatch = FindBestMatch(seeker, seekersByRating, alreadyMatched, startIdx);
            if (bestMatch is null)
            {
                seeker.WavesMissed++;
                continue;
            }

            matches.Add((seeker.Seek.UserId, bestMatch.Seek.UserId));
            alreadyMatched.Add(seeker.Seek.UserId);
            alreadyMatched.Add(bestMatch.Seek.UserId);
        }

        return matches;
    }

    private static RatedPoolMember? FindBestMatch(
        RatedPoolMember seeker,
        List<RatedPoolMember> seekersByRating,
        HashSet<string> alreadyMatched,
        int startIdx
    )
    {
        RatedPoolMember? bestMatch = null;
        int bestScore = int.MaxValue;
        for (int i = startIdx; i < seekersByRating.Count; i++)
        {
            var candidate = seekersByRating[i];
            if (
                seeker.Seek.UserId == candidate.Seek.UserId
                || alreadyMatched.Contains(candidate.Seek.UserId)
            )
                continue;

            if (!seeker.Seek.IsRatingCompatibleWith(candidate.Seek))
                break;

            var score = CalculateScore(seeker, candidate);
            if (score is null)
                continue;
            var scoreValue = score.Value;

            if (bestMatch is null)
            {
                bestMatch = candidate;
                bestScore = scoreValue;
                continue;
            }

            if (scoreValue < bestScore)
            {
                bestMatch = candidate;
                bestScore = scoreValue;
            }
        }

        return bestMatch;
    }

    private static int? CalculateScore(RatedPoolMember a, RatedPoolMember b)
    {
        if (!a.Seek.IsCompatibleWith(b.Seek) || !b.Seek.IsCompatibleWith(a.Seek))
            return null;

        return Math.Abs(a.Seek.Rating.Value - b.Seek.Rating.Value)
            - Math.Max(MissBonus(a), MissBonus(b));
    }

    private static int MissBonus(RatedPoolMember seeker) =>
        Math.Clamp(seeker.WavesMissed * 12, min: 0, max: 400);

    private static int BinarySearchLow(List<RatedPoolMember> list, int value)
    {
        int low = 0;
        int high = list.Count - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            var midRating = list[mid].Seek.Rating;
            if (midRating.Value <= value)
                low = mid + 1;
            else
                high = mid - 1;
        }
        return low;
    }
}
