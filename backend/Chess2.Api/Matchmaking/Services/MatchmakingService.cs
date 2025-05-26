using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingService
{
    Task SeekRatedAsync(AuthedUser user, int baseMinutes, int increment);
    void SeekCasual(string userId, int baseMinutes, int increment);
    void CancelSeek(string userId, int baseMinutes, int increment);
}

public class MatchmakingService(
    IRatingService ratingService,
    ITimeControlTranslator secondsToTimeControl,
    IRequiredActor<RatedMatchmakingActor> matchmakingActor,
    IRequiredActor<CasualMatchmakingActor> casualMatchmakingActor
) : IMatchmakingService
{
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _secondsToTimeControl = secondsToTimeControl;
    private readonly IRequiredActor<RatedMatchmakingActor> _ratedMatchmakingActor =
        matchmakingActor;
    private readonly IRequiredActor<CasualMatchmakingActor> _casualMatchmakingActor =
        casualMatchmakingActor;

    public async Task SeekRatedAsync(AuthedUser user, int baseMinutes, int increment)
    {
        var rating = await _ratingService.GetOrCreateRatingAsync(
            user,
            _secondsToTimeControl.FromSeconds(baseMinutes)
        );

        var poolInfo = new PoolInfo(baseMinutes, increment);
        var command = new MatchmakingCommands.CreateRatedSeek(user.Id, rating.Value, poolInfo);
        _ratedMatchmakingActor.ActorRef.Tell(command);
    }

    public void SeekCasual(string userId, int baseMinutes, int increment)
    {
        var poolInfo = new PoolInfo(baseMinutes, increment);
        var command = new MatchmakingCommands.CreateCasualSeek(userId, poolInfo);
        _casualMatchmakingActor.ActorRef.Tell(command);
    }

    public void CancelSeek(string userId, int baseMinutes, int increment)
    {
        var poolInfo = new PoolInfo(baseMinutes, increment);
        var command = new MatchmakingCommands.CancelSeek(userId, poolInfo);
        _ratedMatchmakingActor.ActorRef.Tell(command);
    }
}
