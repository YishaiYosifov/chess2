using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.Users.Entities;
using Microsoft.Extensions.Options;

namespace Chess2.Api.UserRating.Services;

public interface IRatingService
{
    Task<Rating> GetOrCreateRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        CancellationToken token = default
    );
    Task<Rating> GetOrCreateRatingAsync(
        AuthedUser user,
        TimeControlSettings timeControl,
        CancellationToken token = default
    );
}

public class RatingService(
    ILogger<RatingService> logger,
    IRatingRepository ratingRepository,
    IOptions<AppSettings> settings,
    IUnitOfWork unitOfWork,
    ITimeControlTranslator timeControlTranslator
) : IRatingService
{
    private readonly ILogger<RatingService> _logger = logger;
    private readonly IRatingRepository _ratingRepository = ratingRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly GameSettings _settings = settings.Value.Game;

    public async Task<Rating> GetOrCreateRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        CancellationToken token = default
    )
    {
        var rating = await _ratingRepository.GetTimeControlRatingAsync(user, timeControl, token);
        if (rating is not null)
            return rating;

        _logger.LogInformation(
            "Creating new rating for user {UserId} with time control {TimeControl}",
            user.Id,
            timeControl
        );
        rating = new Rating()
        {
            UserId = user.Id,
            TimeControl = timeControl,
            Value = _settings.DefaultRating,
        };

        await _ratingRepository.AddRatingAsync(rating, user, token);
        await _unitOfWork.CompleteAsync(token);

        return rating;
    }

    public async Task<Rating> GetOrCreateRatingAsync(
        AuthedUser user,
        TimeControlSettings timeControl,
        CancellationToken token = default
    )
    {
        var timeControlEnum = _timeControlTranslator.FromSeconds(timeControl.BaseSeconds);
        var rating = await GetOrCreateRatingAsync(user, timeControlEnum, token);
        return rating;
    }

    public async Task UpdateRatingForResultAsync(
        AuthedUser whiteUser,
        AuthedUser blackUser,
        GameResult result,
        TimeControl timeControl,
        CancellationToken token = default
    )
    {
        var whiteRating = await GetOrCreateRatingAsync(whiteUser, timeControl, token);
        var blackRating = await GetOrCreateRatingAsync(blackUser, timeControl, token);
    }

    private int CalculateNewRating(int playerRating, int opponentRating, int score)
    {
        var expectedScore = 1 / (1 + Math.Pow(10, opponentRating - playerRating) / 400);
    }
}
