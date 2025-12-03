using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Social.Services;
using AnarchyChess.Api.UserRating.Services;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Matchmaking.Services;

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
    Task<RatedSeeker> CreateRatedSeekerAsync(
        AuthedUser user,
        TimeControlSettings timeControlSettings,
        int? allowedRatingRange,
        CancellationToken token = default
    );
}

public class SeekerCreator(
    IRatingService ratingService,
    IOptions<AppSettings> settings,
    IBlockService blockService,
    TimeProvider timeProvider
) : ISeekerCreator
{
    private readonly IRatingService _ratingService = ratingService;
    private readonly IBlockService _blockService = blockService;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly LobbySettings _settings = settings.Value.Lobby;

    public async Task<RatedSeeker> CreateRatedSeekerAsync(
        AuthedUser user,
        TimeControlSettings timeControl,
        int? allowedRatingRange,
        CancellationToken token = default
    )
    {
        var rating = await _ratingService.GetRatingAsync(user.Id, timeControl.Type, token);
        var blocked = await _blockService.GetAllBlockedUserIdsAsync(user.Id, token);

        return new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            ExcludeUserIds: blocked,
            CreatedAt: _timeProvider.GetUtcNow(),
            Rating: new(
                Value: rating,
                AllowedRatingRange: allowedRatingRange,
                TimeControl: timeControl.Type
            )
        );
    }

    public async Task<RatedSeeker> CreateRatedSeekerAsync(
        AuthedUser user,
        TimeControlSettings timeControlSettings,
        CancellationToken token = default
    ) =>
        await CreateRatedSeekerAsync(
            user,
            timeControlSettings,
            _settings.AllowedMatchRatingDifference,
            token
        );

    public async Task<OpenRatedSeeker> CreateRatedOpenSeekerAsync(
        AuthedUser user,
        CancellationToken token = default
    )
    {
        Dictionary<TimeControl, int> ratings = [];
        foreach (var timeControl in Enum.GetValues<TimeControl>())
        {
            var rating = await _ratingService.GetRatingAsync(user.Id, timeControl, token);
            ratings[timeControl] = rating;
        }
        var blocked = await _blockService.GetAllBlockedUserIdsAsync(user.Id, token);

        return new(
            UserId: user.Id,
            UserName: user.UserName ?? "unknown",
            ExcludeUserIds: blocked,
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
            ExcludeUserIds: await _blockService.GetAllBlockedUserIdsAsync(user.Id, token),
            CreatedAt: _timeProvider.GetUtcNow()
        );

    public CasualSeeker CreateGuestCasualSeeker(UserId userId) =>
        new(
            UserId: userId,
            UserName: "Guest",
            ExcludeUserIds: [],
            CreatedAt: _timeProvider.GetUtcNow()
        );
}
