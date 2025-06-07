using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.DTOs;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using ErrorOr;

namespace Chess2.Api.Game.Services;

public interface IGameService
{
    Task<ErrorOr<GameStateDto>> GetGameStateAsync(
        string gameToken,
        CancellationToken token = default
    );
    Task<string> StartGameAsync(
        string userId1,
        IActorRef user1Actor,
        string userId2,
        IActorRef user2Actor
    );
}

public class GameService(
    IRequiredActor<GameActor> gameActor,
    IGameTokenGenerator gameTokenGenerator
) : IGameService
{
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;
    private readonly IGameTokenGenerator _gameTokenGenerator = gameTokenGenerator;

    public async Task<string> StartGameAsync(
        string userId1,
        IActorRef user1Actor,
        string userId2,
        IActorRef user2Actor
    )
    {
        var token = await _gameTokenGenerator.GenerateUniqueGameToken();
        await _gameActor.ActorRef.Ask<GameEvents.GameStartedEvent>(
            new GameCommands.StartGame(
                token,
                WhiteId: userId1,
                WhiteActor: user1Actor,
                BlackId: userId2,
                BlackActor: user2Actor
            )
        );

        return token;
    }

    public async Task<ErrorOr<GameStateDto>> GetGameStateAsync(
        string gameToken,
        CancellationToken token = default
    )
    {
        var gameStartedResult = await ChechGameStartedAsync(gameToken, token);
        if (gameStartedResult.IsError)
            return gameStartedResult.Errors;

        var state = await _gameActor.ActorRef.Ask<GameEvents.GameStateEvent>(
            new GameQueries.GetGameState(gameToken),
            token
        );
        return state.State;
    }

    private async Task<ErrorOr<Success>> ChechGameStartedAsync(
        string gameToken,
        CancellationToken token = default
    )
    {
        var gameStatus = await _gameActor.ActorRef.Ask<GameEvents.GameStatusEvent>(
            new GameQueries.GetGameStatus(gameToken),
            token
        );

        return gameStatus.Status is GameStatus.NotStarted
            ? GameErrors.GameNotFound
            : Result.Success;
    }
}
