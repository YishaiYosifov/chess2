using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Models;
using Chess2.Api.Shared.Services;

namespace Chess2.Api.Game.Services;

public interface IGameTokenGenerator
{
    Task<string> GenerateUniqueGameToken();
}

public class GameTokenGenerator(
    IRequiredActor<GameActor> gameActor,
    IRandomCodeGenerator randomCodeGenerator
) : IGameTokenGenerator
{
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;
    private readonly IRandomCodeGenerator _randomCodeGenerator = randomCodeGenerator;

    public async Task<string> GenerateUniqueGameToken()
    {
        while (true)
        {
            var token = _randomCodeGenerator.GenerateBase62Code(16);
            var isGameOngoing = await _gameActor.ActorRef.Ask<bool>(
                new GameQueries.IsGameOngoing(token)
            );
            if (!isGameOngoing)
                return token;
        }
    }
}
