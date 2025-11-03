using Chess2.Api.Profile.Models;

namespace Chess2.Api.Tournaments.Models;

public record TournamentPlayerGrainKey(TournamentToken TournamentId, UserId UserId)
{
    public string ToKey() => $"{TournamentId}:{UserId}";

    public static TournamentPlayerGrainKey FromKey(string grainKeyString)
    {
        var parts = grainKeyString.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid grain key string format", nameof(grainKeyString));

        return new TournamentPlayerGrainKey(parts[0], parts[1]);
    }
}
