using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Services.Pools;

public interface IRatedMatchmakingPool : IMatchmakingPool
{
    void AddSeek(string userId, int rating);
}

public class SeekInfo(string userId, int rating, int wavesMissed = 0)
{
    public string UserId { get; } = userId;
    public int Rating { get; } = rating;
    public int WavesMissed { get; set; } = wavesMissed;
}

public class RatedMatchmakingPool(IOptions<AppSettings> settings) : IRatedMatchmakingPool
{
    private readonly GameSettings _settings = settings.Value.Game;
    private readonly Dictionary<string, SeekInfo> _seekers = [];

    public IReadOnlyList<string> Seekers => _seekers.Keys.ToList().AsReadOnly();
    public int SeekerCount => _seekers.Count;

    public virtual void AddSeek(string userId, int rating)
    {
        var seekInfo = new SeekInfo(userId, rating);
        _seekers[userId] = seekInfo;
    }

    public virtual bool RemoveSeek(string userId) => _seekers.Remove(userId);

    public List<(string userId1, string userId2)> CalculateMatches()
    {
        var matches = new List<(string, string)>();
        var alreadyMatched = new HashSet<string>();

        var seekersByRating = _seekers.Values.OrderBy(seeker => seeker.Rating).ToList();
        var seekersByMissedWaves = _seekers
            .Values.OrderByDescending(seeker => seeker.WavesMissed)
            .ToList();

        foreach (var seeker in seekersByMissedWaves)
        {
            if (alreadyMatched.Contains(seeker.UserId))
                continue;

            var (startIdx, endIdx) = GetSeekerSearchRatingBounds(seeker, seekersByRating);

            SeekInfo? bestMatch = null;
            int bestRatingDifference = int.MaxValue;
            for (int i = startIdx; i <= endIdx; i++)
            {
                var candidate = seekersByRating[i];
                if (seeker.UserId == candidate.UserId || alreadyMatched.Contains(candidate.UserId))
                    continue;

                var ratingDifference = Math.Abs(candidate.Rating - seeker.Rating);
                if (bestMatch is null)
                {
                    bestMatch = candidate;
                    bestRatingDifference = ratingDifference;
                    continue;
                }

                bool isCandidateOlder = candidate.WavesMissed > bestMatch.WavesMissed;
                bool isCandidateCloser =
                    candidate.WavesMissed == bestMatch.WavesMissed
                    && ratingDifference < bestRatingDifference;
                if (isCandidateOlder || isCandidateCloser)
                {
                    bestMatch = candidate;
                    bestRatingDifference = ratingDifference;
                }
            }

            if (bestMatch is null)
            {
                seeker.WavesMissed++;
                continue;
            }

            matches.Add((seeker.UserId, bestMatch.UserId));
            alreadyMatched.Add(seeker.UserId);
            alreadyMatched.Add(bestMatch.UserId);
        }
        RemoveSeekBatch(alreadyMatched);

        return matches;
    }

    private void RemoveSeekBatch(IEnumerable<string> seeks)
    {
        foreach (var seek in seeks)
            RemoveSeek(seek);
    }

    private (int startIdx, int endIdx) GetSeekerSearchRatingBounds(
        SeekInfo seeker,
        List<SeekInfo> seekersByRating
    )
    {
        var seekerRange = CalculateRatingRange(seeker);
        var minRating = seeker.Rating - seekerRange;
        var maxRating = seeker.Rating + seekerRange;

        var (startIdx, _) = BinarySearch(seekersByRating, minRating);
        var (_, endIdx) = BinarySearch(seekersByRating, maxRating);
        return (startIdx, endIdx);
    }

    private static (int low, int high) BinarySearch(List<SeekInfo> list, int value)
    {
        int low = 0,
            high = list.Count - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (list[mid].Rating <= value)
                low = mid + 1;
            else
                high = mid - 1;
        }
        return (low, high);
    }

    private int CalculateRatingRange(SeekInfo seeker) =>
        _settings.StartingMatchRatingDifference
        + seeker.WavesMissed * _settings.MatchRatingDifferenceGrowthPerWave;
}
