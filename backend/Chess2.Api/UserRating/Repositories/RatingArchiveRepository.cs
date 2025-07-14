using Chess2.Api.Game.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.UserRating.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.UserRating.Repositories;

public interface IRatingArchiveRepository
{
    Task AddRatingAsync(RatingArchive rating, CancellationToken token = default);
    Task<List<RatingArchive>> GetArchivesAsync(
        string userId,
        TimeControl timeControl,
        DateTime since,
        CancellationToken token = default
    );
}

public class RatingArchiveRepository(ApplicationDbContext dbContext) : IRatingArchiveRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddRatingAsync(RatingArchive rating, CancellationToken token = default) =>
        await _dbContext.RatingArchives.AddAsync(rating, token);

    public Task<List<RatingArchive>> GetArchivesAsync(
        string userId,
        TimeControl timeControl,
        DateTime since,
        CancellationToken token = default
    ) =>
        _dbContext
            .RatingArchives.Where(r =>
                r.UserId == userId && r.TimeControl == timeControl && r.AchievedAt > since
            )
            .OrderBy(x => x.AchievedAt)
            .ToListAsync(token);
}
