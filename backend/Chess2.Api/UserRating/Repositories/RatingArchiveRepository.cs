using Chess2.Api.Infrastructure;
using Chess2.Api.UserRating.Entities;

namespace Chess2.Api.UserRating.Repositories;

public interface IRatingArchiveRepository
{
    Task AddRatingAsync(RatingArchive rating, CancellationToken token = default);
}

public class RatingArchiveRepository(ApplicationDbContext dbContext) : IRatingArchiveRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddRatingAsync(RatingArchive rating, CancellationToken token = default) =>
        await _dbContext.RatingArchives.AddAsync(rating, token);
}
