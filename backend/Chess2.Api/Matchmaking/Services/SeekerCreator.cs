using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Social.Services;
using Chess2.Api.UserRating.Services;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Services;

public interface ISeekerCreator
{
    Task<CasualSeeker> CreateAuthedCasualSeekerAsync(
        AuthedUser user,
        CancellationToken token = default
    );
    CasualSeeker CreateGuestCasualSeeker(UserId userId);
    Task<RatedSeeker> CreateRatedSeekerAsync(
        AuthedUser user,
        TimeControlSettings timeControl,
        CancellationToken token = default
    );
    Task<OpenRatedSeeker> CreateRatedOpenSeekerAsync(
        AuthedUser user,
        CancellationToken token = default
    );
}

public class SeekerCreator(
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator,
    IOptions<AppSettings> settings,
    IBlockService blockService,
    TimeProvider timeProvider
) : ISeekerCreator
{
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IBlockService _blockService = blockService;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly LobbySettings _settings = settings.Value.Lobby;

    public async Task<RatedSeeker> CreateRatedSeekerAsync(
        AuthedUser user,
        TimeControlSettings timeControlSettings,
        CancellationToken token = default
    )
    {
        var timeControl = _timeControlTranslator.FromSeconds(timeControlSettings.BaseSeconds);
        var rating = await _ratingService.GetRatingAsync(user, timeControl, token);
        var blocked = await _blockService.GetAllBlockedUserIdsAsync(user.Id, token);

        return new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: blocked,
            CreatedAt: _timeProvider.GetUtcNow(),
            Rating: new(
                Value: rating,
                AllowedRatingRange: _settings.AllowedMatchRatingDifference,
                TimeControl: timeControl
            )
        );
    }

    public async Task<OpenRatedSeeker> CreateRatedOpenSeekerAsync(
        AuthedUser user,
        CancellationToken token = default
    )
    {
        Dictionary<TimeControl, int> ratings = [];
        foreach (var timeControl in Enum.GetValues<TimeControl>())
        {
            var rating = await _ratingService.GetRatingAsync(user, timeControl, token);
            ratings[timeControl] = rating;
        }
        var blocked = await _blockService.GetAllBlockedUserIdsAsync(user.Id, token);

        return new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: blocked,
            CreatedAt: _timeProvider.GetUtcNow(),
            Ratings: ratings
        );
    }

    public async Task<CasualSeeker> CreateAuthedCasualSeekerAsync(
        AuthedUser user,
        CancellationToken token = default
    ) =>
        new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            BlockedUserIds: await _blockService.GetAllBlockedUserIdsAsync(user.Id, token),
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
