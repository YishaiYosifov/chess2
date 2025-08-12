using System.Security.Claims;
using Chess2.Api.Auth.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Services;

public interface ISeekerCreator
{
    Seeker CreateAuthedCasualSeeker(AuthedUser user);
    Seeker CreateGuestCasualSeeker(string userId);
    Task<ErrorOr<Seeker>> CreateCasualSeekerAsync(ClaimsPrincipal? userClaims);
    Task<RatedSeeker> CreateRatedSeekerAsync(AuthedUser user, TimeControlSettings timeControl);
}

public class SeekerCreator(
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator,
    IOptions<AppSettings> settings,
    TimeProvider timeProvider,
    IGuestService guestService,
    IAuthService authService
) : ISeekerCreator
{
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IGuestService _guestService = guestService;
    private readonly IAuthService _authService = authService;
    private readonly GameSettings _settings = settings.Value.Game;

    public async Task<RatedSeeker> CreateRatedSeekerAsync(
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

    public async Task<ErrorOr<Seeker>> CreateCasualSeekerAsync(ClaimsPrincipal? userClaims)
    {
        var userIdResult = _authService.GetUserId(userClaims);
        if (userIdResult.IsError)
            return userIdResult.Errors;

        if (_guestService.IsGuest(userClaims))
            return CreateGuestCasualSeeker(userIdResult.Value);

        var authedUserResult = await _authService.GetLoggedInUserAsync(userClaims);
        if (authedUserResult.IsError)
            return authedUserResult.Errors;
        return CreateAuthedCasualSeeker(authedUserResult.Value);
    }

    public Seeker CreateAuthedCasualSeeker(AuthedUser user) =>
        new(
            UserId: user.Id,
            UserName: user?.UserName ?? "unknown",
            BlockedUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow()
        );

    public Seeker CreateGuestCasualSeeker(string userId) =>
        new(
            UserId: userId,
            UserName: "Guest",
            BlockedUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow()
        );
}
