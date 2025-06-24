using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.DTOs;
using Chess2.Api.Game.Errors;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Game.Services;

public interface IGameService
{
    Task<ErrorOr<Success>> CheckGameStartedAsync(
        string gameToken,
        CancellationToken token = default
    );
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
    Task<string> StartGameAsync(string userId1, string userId2, TimeControlSettings timeControl);
}

public class GameService(
    ILogger<GameService> logger,
    IRequiredActor<GameActor> gameActor,
    IGameTokenGenerator gameTokenGenerator,
    UserManager<AuthedUser> userManager,
    IRatingService ratingService
) : IGameService
{
    private readonly ILogger<GameService> _logger = logger;
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;
    private readonly IGameTokenGenerator _gameTokenGenerator = gameTokenGenerator;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;

    public async Task<string> StartGameAsync(
        string userId1,
        string userId2,
        TimeControlSettings timeControl
    )
    {
        var token = await _gameTokenGenerator.GenerateUniqueGameToken();
        await _gameActor.ActorRef.Ask<GameEvents.GameStartedEvent>(
            new GameCommands.StartGame(
                token,
                WhiteId: userId1,
                BlackId: userId2,
                TimeControl: timeControl
            )
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

        var stateResult = await _gameActor.ActorRef.Ask<ErrorOr<GameEvents.GameStateEvent>>(
            new GameQueries.GetGameState(gameToken, userId),
            token
        );
        if (stateResult.IsError)
            return stateResult.Errors;
        var state = stateResult.Value.State;

        var white = await EnrichGamePlayerAsync(state.PlayerWhite, state.TimeControl);
        var black = await EnrichGamePlayerAsync(state.PlayerBlack, state.TimeControl);

        return new GameStateDto(white, black, state);
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

    public async Task<ErrorOr<Success>> EndGameAsync(
        string gameToken,
        string userId,
        CancellationToken token = default
    )
    {
        var gameStartedResult = await CheckGameStartedAsync(gameToken, token);
        if (gameStartedResult.IsError)
            return gameStartedResult.Errors;

        _gameActor.ActorRef.Tell(new GameCommands.EndGame(gameToken, userId));
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> CheckGameStartedAsync(
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

    private async Task<GamePlayerDto> EnrichGamePlayerAsync(
        GamePlayer player,
        TimeControlSettings timeControl
    )
    {
        var user = await _userManager.FindByIdAsync(player.UserId);
        var rating = user is not null
            ? await _ratingService.GetOrCreateRatingAsync(user, timeControl)
            : null;

        return new GamePlayerDto(
            player,
            userName: user?.UserName ?? "Guest",
            countryCode: user?.CountryCode,
            rating: rating?.Value
        );
    }
}
