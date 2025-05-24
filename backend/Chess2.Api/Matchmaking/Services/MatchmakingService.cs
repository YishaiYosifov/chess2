using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.SignalR;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingService
{
    Task SeekAsync(AuthedUser user, int timeControl, int increment);
    Task SeekGuestAsync(string id, int timeControl, int increment);
    void CancelSeek(string userId, int baseMinutes, int increment);
}

public class MatchmakingService(
    IRatingRepository ratingRepository,
    IHubContext<MatchmakingHub, IMatchmakingClient> matchmakingHubCtx,
    ITimeControlTranslator secondsToTimeControl,
    IRequiredActor<MatchmakingActor> matchmakingActor
) : IMatchmakingService
{
    private readonly IRatingRepository _ratingRepository = ratingRepository;
    private readonly IHubContext<MatchmakingHub, IMatchmakingClient> _matchmakingHubCtx =
        matchmakingHubCtx;
    private readonly ITimeControlTranslator _secondsToTimeControl = secondsToTimeControl;
    private readonly IRequiredActor<MatchmakingActor> _matchmakingActor = matchmakingActor;

    public async Task SeekAsync(AuthedUser user, int baseMinutes, int increment)
    {
        var rating = await _ratingRepository.GetTimeControlRatingAsync(
            user,
            _secondsToTimeControl.FromSeconds(baseMinutes)
        );

        var timeControl = new TimeControlInfo(baseMinutes, increment);
        var command = new MatchmakingCommands.CreateSeek(user.Id, rating.Value, timeControl);
        _matchmakingActor.ActorRef.Tell(command);
    }

    public Task SeekGuestAsync(string id, int timeControl, int increment)
    {
        throw new NotImplementedException();
    }

    public void CancelSeek(string userId, int baseMinutes, int increment)
    {
        var timeControl = new TimeControlInfo(baseMinutes, increment);
        var command = new MatchmakingCommands.CancelSeek(userId, timeControl);
        _matchmakingActor.ActorRef.Tell(command);
    }

    private async Task StartGame(string userId1, string userId2)
    {
        // TODO
        var token = "test";

        var user1 = _matchmakingHubCtx.Clients.User(userId1);
        var user2 = _matchmakingHubCtx.Clients.User(userId2);

        await user1.MatchFoundAsync(token);
        await user2.MatchFoundAsync(token);
    }
}
