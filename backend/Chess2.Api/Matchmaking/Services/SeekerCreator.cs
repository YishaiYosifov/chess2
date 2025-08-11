using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Services;

public interface ISeekerCreator
{
    Seeker CasualSeeker(AuthedUser user);
    Seeker CasualSeeker(string userId);
    Task<RatedSeeker> RatedSeekerAsync(AuthedUser user, TimeControlSettings timeControl);
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
    private readonly GameSettings _settings = settings.Value.Game;

    public async Task<RatedSeeker> RatedSeekerAsync(
        AuthedUser user,
        TimeControlSettings timeControl
    )
    {
        var rating = await _ratingService.GetRatingAsync(
            user,
            _timeControlTranslator.FromSeconds(timeControl.BaseSeconds)
        );

        return new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: [],
            Rating: new(Value: rating, AllowedRatingRange: _settings.AllowedMatchRatingDifference),
            CreatedAt: _timeProvider.GetUtcNow()
        );
    }

    public Seeker CasualSeeker(AuthedUser user) =>
        new(
            UserId: user.Id,
            UserName: user?.UserName ?? "unknown",
            BlockedUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow()
        );

    public Seeker CasualSeeker(string userId) =>
        new(
            UserId: userId,
            UserName: "Guest",
            BlockedUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow()
        );
}
