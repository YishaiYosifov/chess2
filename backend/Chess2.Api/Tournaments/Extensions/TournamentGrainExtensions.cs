using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Grains;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.Extensions;

public static class TournamentGrainExtensions
{
    public static ITournamentFormatGrain GetTournamentFormatGrain(
        this IGrainFactory factory,
        Tournament tournament
    )
    {
        return tournament.Format switch
        {
            TournamentFormat.Arena => factory.GetGrain<IArenaTournamentFormatGrain>(
                tournament.TournamentToken
            ),
            _ => throw new InvalidOperationException(
                $"Unsupported tournament type: {tournament.Format}"
            ),
        };
    }
}
