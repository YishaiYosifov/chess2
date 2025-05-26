using Chess2.Api.Game.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.UserRating.Repositories;

public interface IRatingRepository
{
    Task<Rating?> GetTimeControlRatingAsync(AuthedUser user, TimeControl timeControl);
    Task AddRating(Rating rating, AuthedUser user);
}

public class RatingRepository(ApplicationDbContext dbContext) : IRatingRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Rating?> GetTimeControlRatingAsync(
        AuthedUser user,
        TimeControl timeControl
    ) =>
        await _dbContext
            .Ratings.Where(rating => rating.User == user && rating.TimeControl == timeControl)
            .SingleOrDefaultAsync();

    public async Task AddRating(Rating rating, AuthedUser user)
    {
        user.Ratings.Add(rating);
        await _dbContext.AddAsync(rating);
    }
}
