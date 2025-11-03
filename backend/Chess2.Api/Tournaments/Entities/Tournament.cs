using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.Entities;

public class Tournament
{
    public int Id { get; set; }
    public required TournamentToken Token { get; set; }

    public required UserId HostedBy { get; set; }
    public required PoolType PoolType { get; set; }
    public required int BaseSeconds { get; set; }
    public required int IncrementSeconds { get; set; }
}
