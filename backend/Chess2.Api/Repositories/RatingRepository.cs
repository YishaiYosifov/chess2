using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public interface IRatingRepository
{
    Task<List<Rating>> GetAllRatings(AuthedUser user);
    Task<Rating> GetTimeControlRating(AuthedUser user, TimeControl timeControl);
}

public class RatingRepository(Chess2DbContext dbContext) : IRatingRepository
{
    private readonly Chess2DbContext _dbContext = dbContext;

    public Task<List<Rating>> GetAllRatings(AuthedUser user) =>
        _dbContext.Ratings
            .Where(rating => rating.User == user)
            .ToListAsync();

    public Task<Rating> GetTimeControlRating(AuthedUser user, TimeControl timeControl) =>
        _dbContext.Ratings
            .Where(rating => rating.User == user && rating.TimeControl == timeControl)
            .SingleAsync();
}
