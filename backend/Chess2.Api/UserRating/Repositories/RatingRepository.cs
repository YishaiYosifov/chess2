using Chess2.Api.Infrastructure;
using Chess2.Api.Models;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.UserRating.Repositories;

public interface IRatingRepository
{
    Task<List<Rating>> GetAllRatingsAsync(AuthedUser user);
    Task<Rating> GetTimeControlRatingAsync(AuthedUser user, TimeControl timeControl);
}

public class RatingRepository(ApplicationDbContext dbContext) : IRatingRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<List<Rating>> GetAllRatingsAsync(AuthedUser user) =>
        _dbContext.Ratings.Where(rating => rating.User == user).ToListAsync();

    public async Task<Rating> GetTimeControlRatingAsync(AuthedUser user, TimeControl timeControl) =>
        await _dbContext
            .Ratings.Where(rating => rating.User == user && rating.TimeControl == timeControl)
            .SingleOrDefaultAsync() ?? await CreateRating(user, timeControl);

    private async Task<Rating> CreateRating(AuthedUser user, TimeControl timeControl)
    {
        var rating = new Rating() { User = user, TimeControl = timeControl };
        user.Ratings.Add(rating);

        await _dbContext.AddAsync(rating);
        await _dbContext.SaveChangesAsync();

        return rating;
    }
}
