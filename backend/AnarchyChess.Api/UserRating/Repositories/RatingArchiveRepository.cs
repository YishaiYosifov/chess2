using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.UserRating.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.UserRating.Repositories;

public interface IRatingArchiveRepository
{
    Task AddRatingAsync(RatingArchive rating, CancellationToken token = default);
    Task<List<RatingArchive>> GetArchivesAsync(
        UserId userId,
        TimeControl timeControl,
        DateTime since,
        CancellationToken token = default
    );
    Task<RatingArchive?> GetHighestAsync(
        UserId userId,
        TimeControl timeControl,
        CancellationToken token = default
    );
    Task<RatingArchive?> GetLowestAsync(
        UserId userId,
        TimeControl timeControl,
        CancellationToken token = default
    );
}

public class RatingArchiveRepository(ApplicationDbContext dbContext) : IRatingArchiveRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddRatingAsync(RatingArchive rating, CancellationToken token = default) =>
        await _dbContext.RatingArchives.AddAsync(rating, token);

    public Task<List<RatingArchive>> GetArchivesAsync(
        UserId userId,
        TimeControl timeControl,
        DateTime since,
        CancellationToken token = default
    ) =>
        _dbContext
            .RatingArchives.Where(x =>
                x.UserId == userId && x.TimeControl == timeControl && x.AchievedAt > since
            )
            .OrderBy(x => x.AchievedAt)
            .ToListAsync(token);

    public Task<RatingArchive?> GetHighestAsync(
        UserId userId,
        TimeControl timeControl,
        CancellationToken token = default
    ) =>
        _dbContext
            .RatingArchives.Where(r => r.UserId == userId && r.TimeControl == timeControl)
            .OrderByDescending(x => x.Value)
            .FirstOrDefaultAsync(token);

    public Task<RatingArchive?> GetLowestAsync(
        UserId userId,
        TimeControl timeControl,
        CancellationToken token = default
    ) =>
        _dbContext
            .RatingArchives.Where(r => r.UserId == userId && r.TimeControl == timeControl)
            .OrderBy(x => x.Value)
            .FirstOrDefaultAsync(token);
}
