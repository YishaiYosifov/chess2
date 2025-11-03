using Chess2.Api.Infrastructure;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Tournaments.Repositories;

public interface ITournamentRepository
{
    Task AddTournamentAsync(Tournament tournament, CancellationToken token = default);
    Task GetTournamentByToken(TournamentToken tournamentToken, CancellationToken token = default);
}

public class TournamentRepository(ApplicationDbContext dbContext) : ITournamentRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddTournamentAsync(
        Tournament tournament,
        CancellationToken token = default
    ) => await _dbContext.Tournaments.AddAsync(tournament, token);

    public Task GetTournamentByToken(
        TournamentToken tournamentToken,
        CancellationToken token = default
    ) =>
        _dbContext.Tournaments.FirstOrDefaultAsync(
            x => x.TournamentToken == tournamentToken,
            token
        );
}
