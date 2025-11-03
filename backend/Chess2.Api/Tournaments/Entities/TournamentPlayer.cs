using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.Entities;

public class TournamentPlayer
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }
    public required TournamentToken TournamentToken { get; set; }

    public UserId? LastPlayedOpponent { get; set; }
    public required int Rating { get; set; }

    public int Score { get; set; } = 0;
}
