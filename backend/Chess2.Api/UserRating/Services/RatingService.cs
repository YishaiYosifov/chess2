using Chess2.Api.Game.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.UserRating.Models;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.Users.Entities;
using Microsoft.Extensions.Options;

namespace Chess2.Api.UserRating.Services;

public interface IRatingService
{
    Task UpdateRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        int newRating,
        CancellationToken token = default
    );
    Task<int> GetRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        CancellationToken token = default
    );
    Task<RatingChange> UpdateRatingForResultAsync(
        AuthedUser whiteUser,
        AuthedUser blackUser,
        GameResult result,
        TimeControl timeControl,
        CancellationToken token = default
    );
}

public class RatingService(
    ILogger<RatingService> logger,
    ICurrentRatingRepository currentRatingRepository,
    IRatingArchiveRepository ratingArchiveRepository,
    IOptions<AppSettings> settings
) : IRatingService
{
    private readonly ILogger<RatingService> _logger = logger;
    private readonly ICurrentRatingRepository _currentRatingRepository = currentRatingRepository;
    private readonly IRatingArchiveRepository _ratingArchiveRepository = ratingArchiveRepository;
    private readonly GameSettings _settings = settings.Value.Game;

    public async Task<int> GetRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        CancellationToken token = default
    )
    {
        var rating = await _currentRatingRepository.GetRatingAsync(user.Id, timeControl, token);
        return rating is null ? _settings.DefaultRating : rating.Value;
    }

    public async Task UpdateRatingAsync(
        AuthedUser user,
        TimeControl timeControl,
        int newRating,
        CancellationToken token = default
    )
    {
        CurrentRating rating = new()
        {
            UserId = user.Id,
            TimeControl = timeControl,
            Value = newRating,
        };
        RatingArchive ratingArchive = new()
        {
            UserId = user.Id,
            TimeControl = timeControl,
            Value = newRating,
        };

        await _currentRatingRepository.UpsertRatingAsync(rating, token);
        await _ratingArchiveRepository.AddRatingAsync(ratingArchive, token);
    }

    public async Task<RatingChange> UpdateRatingForResultAsync(
        AuthedUser whiteUser,
        AuthedUser blackUser,
        GameResult result,
        TimeControl timeControl,
        CancellationToken token = default
    )
    {
        var whiteRating = await GetRatingAsync(whiteUser, timeControl, token);
        var blackRating = await GetRatingAsync(blackUser, timeControl, token);

        var ratingChange = CalculateRatingChange(whiteRating, blackRating, result);
        if (ratingChange.WhiteChange != 0)
            await UpdateRatingAsync(
                whiteUser,
                timeControl,
                whiteRating + ratingChange.WhiteChange,
                token
            );
        if (ratingChange.BlackChange != 0)
            await UpdateRatingAsync(
                blackUser,
                timeControl,
                blackRating + ratingChange.BlackChange,
                token
            );

        return ratingChange;
    }

    private RatingChange CalculateRatingChange(int whiteRating, int blackRating, GameResult result)
    {
        var whiteScore = result switch
        {
            GameResult.WhiteWin => 1.0,
            GameResult.BlackWin => 0.0,
            GameResult.Draw => 0.5,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
        var expectedWhiteScore = 1 / (1 + Math.Pow(10, (blackRating - whiteRating) / 400.0));
        var whiteRatingChange = (int)
            Math.Round(_settings.KFactor * (whiteScore - expectedWhiteScore));
        var blackRatingChange = -whiteRatingChange;

        if (whiteRating + whiteRatingChange < 100)
            whiteRatingChange = -Math.Abs(whiteRating - 100);
        if (blackRating + blackRatingChange < 100)
            blackRatingChange = -Math.Abs(blackRating - 100);

        return new(whiteRatingChange, blackRatingChange);
    }
}
