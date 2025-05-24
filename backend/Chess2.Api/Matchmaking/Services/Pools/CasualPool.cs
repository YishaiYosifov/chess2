namespace Chess2.Api.Matchmaking.Services.Pools;

public class CasualMatchmakingPool : MatchmakingPool
{
    public override List<(string userId1, string userId2)> CalculateMatches()
    {
        var seekersByMissedWaves = _seekers
            .Values.OrderByDescending(seeker => seeker.WavesMissed)
            .ToList();

        var matches = new List<(string, string)>();
        for (int i = 0; i < seekersByMissedWaves.Count; i += 2)
        {
            if (i + 1 >= seekersByMissedWaves.Count)
            {
                // Odd number of seekers, last one remains unmatched
                seekersByMissedWaves[i].WavesMissed++;
                break;
            }

            var seeker1 = seekersByMissedWaves[i];
            var seeker2 = seekersByMissedWaves[i + 1];

            matches.Add((seeker1.UserId, seeker2.UserId));
        }

        return matches;
    }
}
