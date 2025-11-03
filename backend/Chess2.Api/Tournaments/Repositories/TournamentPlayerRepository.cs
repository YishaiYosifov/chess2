using Chess2.Api.Infrastructure;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Tournaments.Repositories;

public interface ITournamentPlayerRepository
{
    Task AddPlayerAsync(TournamentPlayer player, CancellationToken token = default);
    Task<List<TournamentPlayer>> GetAllPlayersOfTournamentAsync(
        TournamentToken tournamentToken,
        CancellationToken token
    );
    Task RemovePlayerByIdAsync(UserId userId, CancellationToken token = default);
}

public class TournamentPlayerRepository(ApplicationDbContext dbContext)
    : ITournamentPlayerRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddPlayerAsync(TournamentPlayer player, CancellationToken token = default) =>
        await _dbContext.TournamentPlayers.AddAsync(player, token);

    public Task RemovePlayerByIdAsync(UserId userId, CancellationToken token = default) =>
        _dbContext.TournamentPlayers.Where(x => x.UserId == userId).ExecuteDeleteAsync(token);

    public Task<List<TournamentPlayer>> GetAllPlayersOfTournamentAsync(
        TournamentToken tournamentToken,
        CancellationToken token
    ) =>
        _dbContext
            .TournamentPlayers.Where(x => x.TournamentToken == tournamentToken)
            .ToListAsync(token);
}
