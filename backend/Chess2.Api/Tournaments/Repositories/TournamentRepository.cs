using Chess2.Api.Infrastructure;
using Chess2.Api.Tournaments.Entities;

namespace Chess2.Api.Tournaments.Repositories;

public interface ITournamentRepository
{
    Task AddTournamentAsync(Tournament tournament, CancellationToken token = default);
}

public class TournamentRepository(ApplicationDbContext dbContext) : ITournamentRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddTournamentAsync(
        Tournament tournament,
        CancellationToken token = default
    ) => await _dbContext.Tournaments.AddAsync(tournament, token);
}
