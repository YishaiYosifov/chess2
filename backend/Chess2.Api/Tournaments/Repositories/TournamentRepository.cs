using Chess2.Api.Infrastructure;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Tournaments.Repositories;

public interface ITournamentRepository
{
    Task AddTournamentAsync(Tournament tournament, CancellationToken token = default);
    Task<Tournament?> GetByTokenAsync(
        TournamentToken tournamentToken,
        CancellationToken token = default
    );
    void UpdateTournament(Tournament tournament);
}

public class TournamentRepository(ApplicationDbContext dbContext) : ITournamentRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddTournamentAsync(
        Tournament tournament,
        CancellationToken token = default
    ) => await _dbContext.Tournaments.AddAsync(tournament, token);

    public Task<Tournament?> GetByTokenAsync(
        TournamentToken tournamentToken,
        CancellationToken token = default
    ) =>
        _dbContext
            .Tournaments.Where(x => x.TournamentToken == tournamentToken)
            .SingleOrDefaultAsync(token);

    public void UpdateTournament(Tournament tournament) =>
        _dbContext.Tournaments.Update(tournament);
}
