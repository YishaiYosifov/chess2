using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingPool
{
    int SeekerCount { get; }
    IReadOnlyList<SeekInfo> Seekers { get; }

    void AddSeek(string userId, int rating);
    bool RemoveSeek(string userId);
    List<(string userId1, string userId2)> CalculateMatches();
}

public class MatchmakingPool(IOptions<AppSettings> settings) : IMatchmakingPool
{
    // ordered dictionary so old seeks are prioritized
    private readonly OrderedDictionary<string, SeekInfo> _seekers = [];
    private readonly GameSettings _settings = settings.Value.Game;

    public IReadOnlyList<SeekInfo> Seekers => _seekers.Values.AsReadOnly();
    public int SeekerCount => _seekers.Count;

    public void AddSeek(string userId, int rating)
    {
        var seekInfo = new SeekInfo(userId, rating);
        _seekers[userId] = seekInfo;
    }

    public bool RemoveSeek(string userId) => _seekers.Remove(userId);

    public List<(string userId1, string userId2)> CalculateMatches()
    {
        var matches = new List<(string, string)>();
        var alreadyMatched = new HashSet<string>();
        foreach (var seeker in _seekers.Values)
        {
            if (alreadyMatched.Contains(seeker.UserId))
                continue;

            SeekInfo? match = null;
            foreach (var potentialMatch in _seekers.Values)
            {
                if (seeker == potentialMatch || alreadyMatched.Contains(potentialMatch.UserId))
                    continue;

                var ratingDifference = Math.Abs(seeker.Rating - potentialMatch.Rating);
                var seeker1Range = CalculateRatingRange(seeker);
                var seeker2Range = CalculateRatingRange(potentialMatch);
                if (ratingDifference > seeker1Range || ratingDifference > seeker2Range)
                    continue;

                match = potentialMatch;
                break;
            }

            if (match is not null)
            {
                matches.Add((seeker.UserId, match.UserId));
                alreadyMatched.Add(seeker.UserId);
                alreadyMatched.Add(match.UserId);
            }
            else
            {
                seeker.WavesMissed++;
            }
        }

        return matches;
    }

    private int CalculateRatingRange(SeekInfo seeker) =>
        _settings.StartingMatchRatingDifference
        + seeker.WavesMissed * _settings.MatchRatingDifferenceGrowthPerWave;
}
