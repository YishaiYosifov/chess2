using AnarchyChess.Api.ArchivedGames.Entities;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Pagination.Extensions;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.Models;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.ArchivedGames.Repositories;

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
            .Where(archive => archive.Result != GameResult.Aborted)
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
