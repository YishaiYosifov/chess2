using Chess2.Api.Game.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.UserRating.Repositories;

public interface IRatingRepository
{
    Task<Rating?> GetTimeControlRatingAsync(AuthedUser user, TimeControl timeControl);
    Task AddRatingAsync(Rating rating, AuthedUser user);
}

public class RatingRepository(ApplicationDbContext dbContext) : IRatingRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Rating?> GetTimeControlRatingAsync(
        AuthedUser user,
        TimeControl timeControl
    ) =>
        await _dbContext
            .Ratings
            .Where(rating => rating.UserId == user.Id && rating.TimeControl == timeControl)
            .OrderByDescending(rating => rating.AchievedAt)
            .FirstOrDefaultAsync();

    public async Task AddRatingAsync(Rating rating, AuthedUser user)
    {
        user.Ratings.Add(rating);
        await _dbContext.AddAsync(rating);
    }
}
