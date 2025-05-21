using Chess2.Api.Matchmaking.Repositories;
using Chess2.Api.Matchmaking.SignalR;
using Chess2.Api.Services;
using Chess2.Api.Shared.DTOs;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingService
{
    public Task SeekAsync(AuthedUser user, int timeControl, int increment);
    public Task SeekGuestAsync(string id, int timeControl, int increment);

    public Task CancelSeekAsync(string id);
}

public class MatchmakingService(
    IOptions<AppSettings> settings,
    IRatingRepository ratingRepository,
    IMatchmakingRepository matchmakingRepository,
    IHubContext<MatchmakingHub, IMatchmakingClient> matchmakingHubCtx,
    ITimeControlTranslator secondsToTimeControl
) : IMatchmakingService
{
    private readonly GameSettings _gameSettings = settings.Value.Game;
    private readonly IRatingRepository _ratingRepository = ratingRepository;
    private readonly IMatchmakingRepository _matchmakingRepository = matchmakingRepository;
    private readonly IHubContext<MatchmakingHub, IMatchmakingClient> _matchmakingHubCtx =
        matchmakingHubCtx;
    private readonly ITimeControlTranslator _secondsToTimeControl = secondsToTimeControl;

    public async Task SeekAsync(AuthedUser user, int timeControl, int increment)
    {
        var userId = user.Id.ToString();
        var rating = await _ratingRepository.GetTimeControlRatingAsync(
            user,
            _secondsToTimeControl.FromSeconds(timeControl)
        );
        var matchedUserId = await SearchForMatch(timeControl, increment, rating.Value);
        if (matchedUserId is not null)
        {
            await StartGame(userId, matchedUserId);
            return;
        }

        await _matchmakingRepository.CreateSeekAsync(userId, rating.Value, timeControl, increment);
    }

    public Task SeekGuestAsync(string id, int timeControl, int increment)
    {
        throw new NotImplementedException();
    }

    public Task CancelSeekAsync(string userId) => _matchmakingRepository.CancelSeekAsync(userId);

    private async Task StartGame(string userId1, string userId2)
    {
        // TODO
        var token = "test";

        var user1 = _matchmakingHubCtx.Clients.User(userId1);
        var user2 = _matchmakingHubCtx.Clients.User(userId2);

        await user1.MatchFoundAsync(token);
        await user2.MatchFoundAsync(token);
    }

    private async Task<string?> SearchForMatch(int timeControl, int increment, int rating)
    {
        var range = _gameSettings.MaxMatchRatingDifference;
        var match = await _matchmakingRepository.SearchExistingSeekAsync(
            rating,
            range,
            timeControl,
            increment
        );
        return match;
    }
}
