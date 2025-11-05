using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.TournamentFormats;

public interface ITournamentFormat
{
    TournamentFormat Format { get; }

    void AddPlayer(TournamentPlayerState player);
    void RemovePlayer(UserId userId);
    bool HasPlayer(UserId userId);

    List<(UserId User1, UserId User2)> GetNextMatches();
}
