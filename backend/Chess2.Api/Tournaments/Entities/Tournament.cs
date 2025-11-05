using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Tournaments.Entities;

[PrimaryKey(nameof(TournamentToken))]
public class Tournament
{
    public required TournamentToken TournamentToken { get; set; }

    public required UserId HostedBy { get; set; }
    public required int BaseSeconds { get; set; }
    public required int IncrementSeconds { get; set; }
    public required TournamentFormat Format { get; set; }
}
