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
    Task IncrementScoreFor(
        UserId userId,
        TournamentToken tournamentToken,
        int incrementBy,
        CancellationToken token = default
    );
    Task RemovePlayerFromTournamentAsync(
        UserId userId,
        TournamentToken tournamentToken,
        CancellationToken token = default
    );
}

public class TournamentPlayerRepository(ApplicationDbContext dbContext)
    : ITournamentPlayerRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddPlayerAsync(TournamentPlayer player, CancellationToken token = default) =>
        await _dbContext.TournamentPlayers.AddAsync(player, token);

    public Task IncrementScoreFor(
        UserId userId,
        TournamentToken tournamentToken,
        int incrementBy,
        CancellationToken token = default
    ) =>
        _dbContext
            .TournamentPlayers.Where(x =>
                x.UserId == userId && x.TournamentToken == tournamentToken
            )
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.Score, x => x.Score + incrementBy),
                token
            );

    public Task RemovePlayerFromTournamentAsync(
        UserId userId,
        TournamentToken tournamentToken,
        CancellationToken token = default
    ) =>
        _dbContext
            .TournamentPlayers.Where(x =>
                x.UserId == userId && x.TournamentToken == tournamentToken
            )
            .ExecuteDeleteAsync(token);

    public Task<List<TournamentPlayer>> GetAllPlayersOfTournamentAsync(
        TournamentToken tournamentToken,
        CancellationToken token
    ) =>
        _dbContext
            .TournamentPlayers.Where(x => x.TournamentToken == tournamentToken)
            .ToListAsync(token);
}
