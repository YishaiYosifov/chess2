using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Game.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.Entities;

public class TournamentPlayer
{
    public int Id { get; set; }
    public required TournamentToken TournamentToken { get; set; }

    public required UserId UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required AuthedUser User { get; set; }

    public UserId? LastOpponent { get; set; }
    public required int Rating { get; set; }

    public GameToken? InGame { get; set; }
    public int Score { get; set; } = 0;
}
