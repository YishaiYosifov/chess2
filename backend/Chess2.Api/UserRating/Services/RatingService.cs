using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Shared.Models;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.UserRating.Models;
using Chess2.Api.UserRating.Repositories;
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
    Task<IEnumerable<RatingOverview>> GetRatingOverviewsAsync(
        AuthedUser user,
        DateTime? since,
        CancellationToken token = default
    );
    Task<IEnumerable<CurrentRatingStatus>> GetCurrentRatingsAsync(
        AuthedUser user,
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

    public async Task<IEnumerable<RatingOverview>> GetRatingOverviewsAsync(
        AuthedUser user,
        DateTime? since,
        CancellationToken token = default
    )
    {
        var effectiveSince = DateTime.SpecifyKind(since ?? DateTime.MinValue, DateTimeKind.Utc);
        List<RatingOverview> overviews = [];
        foreach (var timeControl in Enum.GetValues<TimeControl>())
        {
            var overview = await BuildOverviewAsync(user.Id, timeControl, effectiveSince, token);
            if (overview is not null)
                overviews.Add(overview);
        }
        return overviews;
    }

    public async Task<IEnumerable<CurrentRatingStatus>> GetCurrentRatingsAsync(
        AuthedUser user,
        CancellationToken token = default
    )
    {
        List<CurrentRatingStatus> result = [];
        foreach (var timeControl in Enum.GetValues<TimeControl>())
        {
            var rating = await _currentRatingRepository.GetRatingAsync(user.Id, timeControl, token);
            if (rating is not null)
            {
                result.Add(new(TimeControl: timeControl, Rating: rating.Value));
            }
        }
        return result;
    }

    private async Task<RatingOverview?> BuildOverviewAsync(
        string userId,
        TimeControl timeControl,
        DateTime since,
        CancellationToken token = default
    )
    {
        var current = await _currentRatingRepository.GetRatingAsync(userId, timeControl, token);
        if (current is null)
            return null;

        var archives = await _ratingArchiveRepository.GetArchivesAsync(
            userId,
            timeControl,
            since,
            token
        );
        if (archives.Count == 0)
            return null;

        var highest = await _ratingArchiveRepository.GetHighestAsync(userId, timeControl, token);
        var lowest = await _ratingArchiveRepository.GetLowestAsync(userId, timeControl, token);

        var ratings = archives.Select(r => new RatingSummary(r)).ToList();
        return new RatingOverview(
            timeControl,
            ratings,
            Current: current.Value,
            Highest: highest?.Value ?? _settings.DefaultRating,
            Lowest: lowest?.Value ?? _settings.DefaultRating
        );
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
