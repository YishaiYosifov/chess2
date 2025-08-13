using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Chess2.Api.Users.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Services;

public interface ISeekerCreator
{
    CasualSeeker CreateAuthedCasualSeeker(AuthedUser user);
    CasualSeeker CreateGuestCasualSeeker(UserId userId);
    Task<RatedSeeker> CreateRatedSeekerAsync(AuthedUser user, TimeControlSettings timeControl);
    Task<OpenRatedSeeker> CreateRatedOpenSeekerAsync(AuthedUser user);
}

public class SeekerCreator(
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator,
    IOptions<AppSettings> settings,
    TimeProvider timeProvider
) : ISeekerCreator
{
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly LobbySettings _settings = settings.Value.Lobby;

    public async Task<RatedSeeker> CreateRatedSeekerAsync(
        AuthedUser user,
        TimeControlSettings timeControlSettings
    )
    {
        var timeControl = _timeControlTranslator.FromSeconds(timeControlSettings.BaseSeconds);
        var rating = await _ratingService.GetRatingAsync(user, timeControl);

        return new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow(),
            Rating: new(
                Value: rating,
                AllowedRatingRange: _settings.AllowedMatchRatingDifference,
                TimeControl: timeControl
            )
        );
    }

    public async Task<OpenRatedSeeker> CreateRatedOpenSeekerAsync(AuthedUser user)
    {
        Dictionary<TimeControl, int> ratings = [];
        foreach (var timeControl in Enum.GetValues<TimeControl>())
        {
            var rating = await _ratingService.GetRatingAsync(user, timeControl);
            ratings[timeControl] = rating;
        }

        return new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow(),
            Ratings: ratings
        );
    }

    public CasualSeeker CreateAuthedCasualSeeker(AuthedUser user) =>
        new(
            UserId: user.Id,
            UserName: user?.UserName ?? "unknown",
            BlockedUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow()
        );

    public CasualSeeker CreateGuestCasualSeeker(UserId userId) =>
        new(
            UserId: userId,
            UserName: "Guest",
            BlockedUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow()
        );
}
