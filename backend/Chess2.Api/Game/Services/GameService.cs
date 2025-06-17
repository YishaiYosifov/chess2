using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.DTOs;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using ErrorOr;

namespace Chess2.Api.Game.Services;

public interface IGameService
{
    Task<ErrorOr<GameStateDto>> GetGameStateAsync(
        string gameToken,
        string userId,
        CancellationToken token = default
    );
    Task<ErrorOr<GameEvents.PieceMoved>> PerformMoveAsync(
        string gameToken,
        string userId,
        AlgebraicPoint from,
        AlgebraicPoint to,
        CancellationToken token = default
    );
    Task<string> StartGameAsync(string userId1, string userId2);
}

public class GameService(
    ILogger<GameService> logger,
    IRequiredActor<GameActor> gameActor,
    IGameTokenGenerator gameTokenGenerator
) : IGameService
{
    private readonly ILogger<GameService> _logger = logger;
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;
    private readonly IGameTokenGenerator _gameTokenGenerator = gameTokenGenerator;

    public async Task<string> StartGameAsync(string userId1, string userId2)
    {
        var token = await _gameTokenGenerator.GenerateUniqueGameToken();
        await _gameActor.ActorRef.Ask<GameEvents.GameStartedEvent>(
            new GameCommands.StartGame(token, WhiteId: userId1, BlackId: userId2)
        );

        return token;
    }

    public async Task<ErrorOr<GameStateDto>> GetGameStateAsync(
        string gameToken,
        string userId,
        CancellationToken token = default
    )
    {
        var gameStartedResult = await CheckGameStartedAsync(gameToken, token);
        if (gameStartedResult.IsError)
            return gameStartedResult.Errors;

        var state = await _gameActor.ActorRef.Ask<ErrorOr<GameEvents.GameStateEvent>>(
            new GameQueries.GetGameState(gameToken, userId),
            token
        );
        if (state.IsError)
            return state.Errors;

        return state.Value.State;
    }

    public async Task<ErrorOr<GameEvents.PieceMoved>> PerformMoveAsync(
        string gameToken,
        string userId,
        AlgebraicPoint from,
        AlgebraicPoint to,
        CancellationToken token = default
    )
    {
        var gameStartedResult = await CheckGameStartedAsync(gameToken, token);
        if (gameStartedResult.IsError)
            return gameStartedResult.Errors;

        var response = await _gameActor.ActorRef.Ask<ErrorOr<GameEvents.PieceMoved>>(
            new GameCommands.MovePiece(gameToken, userId, from, to),
            token
        );
        return response;
    }

    private async Task<ErrorOr<Success>> CheckGameStartedAsync(
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
