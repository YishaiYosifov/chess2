using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Tournaments.Models;

public record TournamentPlayerState(Seeker Seeker, int Score, GameToken? InGame);
