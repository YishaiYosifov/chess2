using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Tournaments.Entities;

public class TournamentPlayer
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }

    public required int TournamentId { get; set; }

    [ForeignKey(nameof(TournamentId))]
    public required Tournament Tournament { get; set; }

    public UserId? LastPlayedOpponent { get; set; }
    public required int Rating { get; set; }

    public int Score { get; set; } = 0;
}
