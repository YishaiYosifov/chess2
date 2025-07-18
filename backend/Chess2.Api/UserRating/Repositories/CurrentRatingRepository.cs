using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.UserRating.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.UserRating.Repositories;

public interface ICurrentRatingRepository
{
    Task<CurrentRating?> GetRatingAsync(
        string userId,
        TimeControl timeControl,
        CancellationToken token = default
    );
    Task UpsertRatingAsync(CurrentRating rating, CancellationToken token = default);
}

public class CurrentRatingRepository(ApplicationDbContext dbContext) : ICurrentRatingRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<CurrentRating?> GetRatingAsync(
        string userId,
        TimeControl timeControl,
        CancellationToken token = default
    ) =>
        await _dbContext
            .CurrentRatings.Where(rating =>
                rating.UserId == userId && rating.TimeControl == timeControl
            )
            .FirstOrDefaultAsync(token);

    public async Task UpsertRatingAsync(CurrentRating rating, CancellationToken token = default)
    {
        var existing = await GetRatingAsync(rating.UserId, rating.TimeControl, token);
        if (existing is null)
        {
            await _dbContext.CurrentRatings.AddAsync(rating, token);
        }
        else
        {
            existing.Value = rating.Value;
            _dbContext.CurrentRatings.Update(existing);
        }
    }
}
