using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.TournamentFormats;

public interface ITournamentFormat
{
    TournamentFormat Format { get; }

    void GetNextMatches();
}
