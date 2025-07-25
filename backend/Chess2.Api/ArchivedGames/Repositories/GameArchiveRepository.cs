using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.ArchivedGames.Repositories;

public interface IGameArchiveRepository
{
    Task<GameArchive?> GetGameArchiveByTokenAsync(
        string gameToken,
        CancellationToken token = default
    );
    Task AddArchiveAsync(GameArchive gameArchive, CancellationToken token = default);
    Task<List<GameArchive>> GetPaginatedArchivedGamesForUserAsync(
        string userId,
        int take,
        int skip,
        CancellationToken token = default
    );
    Task<int> CountArchivedGamesForUserAsync(string userId, CancellationToken token = default);
}

public class GameArchiveRepository(ApplicationDbContext dbContext) : IGameArchiveRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<GameArchive?> GetGameArchiveByTokenAsync(
        string gameToken,
        CancellationToken token = default
    ) =>
        _dbContext
            .GameArchives.Include(archive => archive.WhitePlayer)
            .Include(archive => archive.BlackPlayer)
            .Include(archive => archive.Moves)
            .ThenInclude(moves => moves.SideEffects)
            .Where(archive => archive.GameToken == gameToken)
            .FirstOrDefaultAsync(token);

    public Task<List<GameArchive>> GetPaginatedArchivedGamesForUserAsync(
        string userId,
        int take,
        int skip,
        CancellationToken token = default
    ) =>
        _dbContext
            .GameArchives.Include(archive => archive.WhitePlayer)
            .Include(archive => archive.BlackPlayer)
            .Where(archive =>
                archive.WhitePlayer.UserId == userId || archive.BlackPlayer.UserId == userId
            )
            .OrderByDescending(archive => archive.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(token);

    public Task<int> CountArchivedGamesForUserAsync(
        string userId,
        CancellationToken token = default
    ) =>
        _dbContext
            .GameArchives.Include(archive => archive.WhitePlayer)
            .Include(archive => archive.BlackPlayer)
            .Where(archive =>
                archive.WhitePlayer.UserId == userId || archive.BlackPlayer.UserId == userId
            )
            .CountAsync(token);

    public async Task AddArchiveAsync(GameArchive gameArchive, CancellationToken token = default) =>
        await _dbContext.GameArchives.AddAsync(gameArchive, token);
}
