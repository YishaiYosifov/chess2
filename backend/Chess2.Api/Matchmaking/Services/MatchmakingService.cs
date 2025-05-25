using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.SignalR;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.Users.Entities;
using Chess2.Api.Users.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingService
{
    Task SeekRatedAsync(AuthedUser user, int baseMinutes, int increment);
    void SeekUnrated(IUser user, int baseMinutes, int increment);
    void CancelSeek(string userId, int baseMinutes, int increment);
}

public class MatchmakingService(
    IRatingRepository ratingRepository,
    IHubContext<MatchmakingHub, IMatchmakingClient> matchmakingHubCtx,
    ITimeControlTranslator secondsToTimeControl,
    IRequiredActor<RatedMatchmakingActor> matchmakingActor,
    IRequiredActor<CasualMatchmakingActor> casualMatchmakingActor
) : IMatchmakingService
{
    private readonly IRatingRepository _ratingRepository = ratingRepository;
    private readonly IHubContext<MatchmakingHub, IMatchmakingClient> _matchmakingHubCtx =
        matchmakingHubCtx;
    private readonly ITimeControlTranslator _secondsToTimeControl = secondsToTimeControl;
    private readonly IRequiredActor<RatedMatchmakingActor> _ratedMatchmakingActor = matchmakingActor;
    private readonly IRequiredActor<CasualMatchmakingActor> _casualMatchmakingActor = casualMatchmakingActor;

    public async Task SeekRatedAsync(AuthedUser user, int baseMinutes, int increment)
    {
        var rating = await _ratingRepository.GetTimeControlRatingAsync(
            user,
            _secondsToTimeControl.FromSeconds(baseMinutes)
        );

        var poolInfo = new PoolInfo(baseMinutes, increment);
        var command = new MatchmakingCommands.CreateRatedSeek(user.Id, rating.Value, poolInfo);
        _ratedMatchmakingActor.ActorRef.Tell(command);
    }

    public void SeekUnrated(IUser user, int baseMinutes, int increment)
    {
        var poolInfo = new PoolInfo(baseMinutes, increment);
        var command = new MatchmakingCommands.CreateCasualSeek(user.Id, poolInfo);
        _casualMatchmakingActor.ActorRef.Tell(command);
    }

    public void CancelSeek(string userId, int baseMinutes, int increment)
    {
        var poolInfo = new PoolInfo(baseMinutes, increment);
        var command = new MatchmakingCommands.CancelSeek(userId, poolInfo);
        _ratedMatchmakingActor.ActorRef.Tell(command);
    }
}

