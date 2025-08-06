using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.PlayerSession.Actors;
using Chess2.Api.PlayerSession.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.Matchmaking.Services;

public interface IMatchmakingService
{
    Task SeekRatedAsync(AuthedUser user, string connectionId, TimeControlSettings timeControl);
    void SeekCasual(string userId, string connectionId, TimeControlSettings timeControl);
    void CancelSeek(string userId, string connectionId);
}

public class MatchmakingService(
    IRatingService ratingService,
    ITimeControlTranslator secondsToTimeControl,
    IRequiredActor<PlayerSessionActor> playerSessionActor
) : IMatchmakingService
{
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _secondsToTimeControl = secondsToTimeControl;
    private readonly IRequiredActor<PlayerSessionActor> _playerSessionActor = playerSessionActor;

    public async Task SeekRatedAsync(
        AuthedUser user,
        string connectionId,
        TimeControlSettings timeControl
    )
    {
        var rating = await _ratingService.GetRatingAsync(
            user,
            _secondsToTimeControl.FromSeconds(timeControl.BaseSeconds)
        );

        var poolCommand = new RatedMatchmakingCommands.CreateRatedSeek(
            user.Id,
            rating,
            timeControl
        );
        var playerSessionCommand = new PlayerSessionCommands.CreateSeek(
            user.Id,
            connectionId,
            poolCommand
        );
        _playerSessionActor.ActorRef.Tell(playerSessionCommand);
    }

    public void SeekCasual(string userId, string connectionId, TimeControlSettings timeControl)
    {
        var poolCommand = new CasualMatchmakingCommands.CreateCasualSeek(userId, timeControl);
        var playerSessionCommand = new PlayerSessionCommands.CreateSeek(
            userId,
            connectionId,
            poolCommand
        );
        _playerSessionActor.ActorRef.Tell(playerSessionCommand);
    }

    public void CancelSeek(string userId, string connectionId)
    {
        var command = new PlayerSessionCommands.CancelSeek(userId, connectionId);
        _playerSessionActor.ActorRef.Tell(command);
    }
}
