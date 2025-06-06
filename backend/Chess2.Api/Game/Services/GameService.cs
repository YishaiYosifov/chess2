using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Models;

namespace Chess2.Api.Game.Services;

public interface IGameService
{
    Task<string> StartGameAsync(string userId1, string userId2);
}

public class GameService(
    IRequiredActor<GameActor> gameActor,
    IGameTokenGenerator gameTokenGenerator
) : IGameService
{
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;
    private readonly IGameTokenGenerator _gameTokenGenerator = gameTokenGenerator;

    public async Task<string> StartGameAsync(string userId1, string userId2)
    {
        var token = await _gameTokenGenerator.GenerateUniqueGameToken();
        await _gameActor.ActorRef.Ask<GameEvents.GameStartedEvent>(
            new GameCommands.StartGame(token, userId1, userId2)
        );

        return token;
    }
}
