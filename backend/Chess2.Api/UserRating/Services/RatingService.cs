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
    Task<Rating> AddRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        int newRating,
        CancellationToken token = default
    );
    Task<Rating> GetOrCreateRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        CancellationToken token = default
    );
    Task UpdateRatingForResultAsync(
        AuthedUser whiteUser,
        AuthedUser blackUser,
        GameResult result,
        TimeControl timeControl,
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

        rating = await AddRatingAsync(user, timeControl, _settings.DefaultRating, token);
        await _unitOfWork.CompleteAsync(token);

        return rating;
    }

    public async Task<Rating> AddRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        int newRating,
        CancellationToken token = default
    )
    {
        var rating = new Rating()
        {
            UserId = user.Id,
            TimeControl = timeControl,
            Value = newRating,
        };
        await _ratingRepository.AddRatingAsync(rating, user, token);
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

        var whiteScore = result switch
        {
            GameResult.WhiteWin => 1.0,
            GameResult.BlackWin => 0.0,
            GameResult.Draw => 0.5,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
        var expectedScore = 1 / (1 + Math.Pow(10, (blackRating.Value - whiteRating.Value) / 400.0));
        var ratingChange = (int)Math.Round(_settings.KFactor * (whiteScore - expectedScore));

        await AddRatingAsync(whiteUser, timeControl, whiteRating.Value + ratingChange, token);
        await AddRatingAsync(blackUser, timeControl, blackRating.Value - ratingChange, token);
    }
}
