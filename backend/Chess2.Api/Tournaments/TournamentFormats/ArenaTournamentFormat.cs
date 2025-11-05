using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.TournamentFormats;

public class ArenaTournamentFormat : ITournamentFormat
{
    public TournamentFormat Format => TournamentFormat.Arena;

    private readonly RatedMatchmakingPool _pool = new();

    public void AddPlayer(TournamentPlayerState player) => _pool.AddSeeker(player.Seeker);

    public void RemovePlayer(UserId userId) => _pool.RemoveSeeker(userId);

    public bool HasPlayer(UserId userId) => _pool.HasSeeker(userId);

    public List<(UserId User1, UserId User2)> GetNextMatches() => _pool.CalculateMatches();
}
