using Chess2.Api.Game.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.Users.Entities;
using Microsoft.Extensions.Options;

namespace Chess2.Api.UserRating.Services;

public interface IRatingService
{
    Task<Rating> GetOrCreateRatingAsync(AuthedUser user, TimeControl timeControl);
}

public class RatingService(
    ILogger<RatingService> logger,
    IRatingRepository ratingRepository,
    IOptions<AppSettings> settings,
    IUnitOfWork unitOfWork
) : IRatingService
{
    private readonly ILogger<RatingService> _logger = logger;
    private readonly IRatingRepository _ratingRepository = ratingRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly GameSettings _settings = settings.Value.Game;

    public async Task<Rating> GetOrCreateRatingAsync(AuthedUser user, TimeControl timeControl)
    {
        var rating = await _ratingRepository.GetTimeControlRatingAsync(user, timeControl);
        if (rating is not null)
            return rating;

        _logger.LogInformation(
            "Creating new rating for user {UserId} with time control {TimeControl}",
            user.Id,
            timeControl
        );
        rating = new Rating()
        {
            User = user,
            UserId = user.Id,
            TimeControl = timeControl,
            Value = _settings.DefaultRating,
        };

        await _ratingRepository.AddRating(rating, user);
        await _unitOfWork.CompleteAsync();

        return rating;
    }
}
