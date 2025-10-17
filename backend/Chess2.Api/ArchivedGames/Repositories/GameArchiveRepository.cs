using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.Infrastructure;
using Chess2.Api.Pagination.Extensions;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.ArchivedGames.Repositories;

public interface IGameArchiveRepository
{
    Task AddArchiveAsync(GameArchive gameArchive, CancellationToken token = default);
    Task<List<GameArchive>> GetPaginatedArchivedGamesForUserAsync(
        UserId userId,
        PaginationQuery pagination,
        CancellationToken token = default
    );
    Task<int> CountArchivedGamesForUserAsync(UserId userId, CancellationToken token = default);
}

public class GameArchiveRepository(ApplicationDbContext dbContext) : IGameArchiveRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<GameArchive>> GetPaginatedArchivedGamesForUserAsync(
        UserId userId,
        PaginationQuery pagination,
        CancellationToken token = default
    ) =>
        _dbContext
            .GameArchives.Include(archive => archive.WhitePlayer)
            .Include(archive => archive.BlackPlayer)
            .Where(archive =>
                archive.WhitePlayer.UserId == userId || archive.BlackPlayer.UserId == userId
            )
            .OrderByDescending(archive => archive.CreatedAt)
            .Paginate(pagination)
            .ToListAsync(token);

    public Task<int> CountArchivedGamesForUserAsync(
        UserId userId,
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
