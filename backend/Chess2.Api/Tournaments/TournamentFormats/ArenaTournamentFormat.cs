using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.TournamentFormats;

public class ArenaTournamentFormat : ITournamentFormat
{
    public TournamentFormat Format => TournamentFormat.Arena;

    public void GetNextMatches()
    {
        throw new NotImplementedException();
    }
}
