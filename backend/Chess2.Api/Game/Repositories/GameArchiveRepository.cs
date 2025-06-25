using Chess2.Api.Game.Entities;
using Chess2.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Game.Repositories;

public interface IGameArchiveRepository
{
    Task<GameArchive?> GetGameArchiveByToken(string gameToken, CancellationToken token = default);
    Task AddArchiveAsync(GameArchive gameArchive, CancellationToken token = default);
}

public class GameArchiveRepository(ApplicationDbContext dbContext) : IGameArchiveRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<GameArchive?> GetGameArchiveByToken(string gameToken, CancellationToken token = default) =>
        _dbContext
            .GameArchives
            .Include(archive => archive.WhitePlayer)
            .Include(archive => archive.BlackPlayer)
            .Include(archive => archive.Moves)
            .Where(archive => archive.GameToken == gameToken)
            .FirstOrDefaultAsync(token);

    public async Task AddArchiveAsync(GameArchive gameArchive, CancellationToken token = default) =>
        await _dbContext.GameArchives.AddAsync(gameArchive, token);
}
