using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Player.Actors;
using Chess2.Api.Player.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingService
{
    Task SeekRatedAsync(AuthedUser user, int baseMinutes, int increment);
    void SeekCasual(string userId, int baseMinutes, int increment);
    void CancelSeek(string userId);
}

public class MatchmakingService(
    IRatingService ratingService,
    ITimeControlTranslator secondsToTimeControl,
    IRequiredActor<PlayerActor> playerActor
) : IMatchmakingService
{
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _secondsToTimeControl = secondsToTimeControl;
    private readonly IRequiredActor<PlayerActor> _playerActor = playerActor;

    public async Task SeekRatedAsync(AuthedUser user, int baseMinutes, int increment)
    {
        var rating = await _ratingService.GetOrCreateRatingAsync(
            user,
            _secondsToTimeControl.FromSeconds(baseMinutes)
        );

        var poolInfo = new PoolInfo(baseMinutes, increment);
        var poolCommand = new RatedMatchmakingCommands.CreateRatedSeek(
            user.Id,
            rating.Value,
            poolInfo
        );
        var playerCommand = new PlayerCommands.CreateSeek(user.Id, poolCommand);
        _playerActor.ActorRef.Tell(playerCommand);
    }

    public void SeekCasual(string userId, int baseMinutes, int increment)
    {
        var poolInfo = new PoolInfo(baseMinutes, increment);
        var poolCommand = new CasualMatchmakingCommands.CreateCasualSeek(userId, poolInfo);
        var playerCommand = new PlayerCommands.CreateSeek(userId, poolCommand);
        _playerActor.ActorRef.Tell(playerCommand);
    }

    public void CancelSeek(string userId)
    {
        var command = new PlayerCommands.CancelSeek(userId);
        _playerActor.ActorRef.Tell(command);
    }
}
