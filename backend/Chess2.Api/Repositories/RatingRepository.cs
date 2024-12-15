using Chess2.Api.Models;
using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Repositories;

public interface IRatingRepository
{
    Task<List<Rating>> GetUserRatings(AuthedUser user);
}

public class RatingRepository(Chess2DbContext dbContext) : IRatingRepository
{
    private readonly Chess2DbContext _dbContext = dbContext;

    public Task<List<Rating>> GetUserRatings(AuthedUser user)
    {
        return _dbContext
            .Ratings
            .Where(rating => rating.User == user)
            .ToListAsync();
    }
}
