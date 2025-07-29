using Akka.Actor;
using Akka.Hosting;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.Shared.Extensions;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.LiveGame.Services;

public interface ILiveGameService
{
    Task<ErrorOr<Success>> EndGameAsync(
        string gameToken,
        string userId,
        CancellationToken token = default
    );
    Task<ErrorOr<GameResponses.GamePlayers>> GetGamePlayersAsync(
        string gameToken,
        CancellationToken token = default
    );
    Task<ErrorOr<GameState>> GetGameStateAsync(
        string gameToken,
        string userId,
        CancellationToken token = default
    );
    Task<bool> IsGameOngoingAsync(string gameToken, CancellationToken token = default);
    Task<ErrorOr<Success>> MakeMoveAsync(
        string gameToken,
        string userId,
        AlgebraicPoint from,
        AlgebraicPoint to,
        CancellationToken token = default
    );
    Task<string> StartGameAsync(
        string userId1,
        string userId2,
        TimeControlSettings timeControl,
        bool isRated
    );
}

public class LiveGameService(
    IRequiredActor<GameActor> gameActor,
    IGameTokenGenerator gameTokenGenerator,
    UserManager<AuthedUser> userManager,
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator
) : ILiveGameService
{
    private readonly IRequiredActor<GameActor> _gameActor = gameActor;
    private readonly IGameTokenGenerator _gameTokenGenerator = gameTokenGenerator;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;

    public async Task<bool> IsGameOngoingAsync(string gameToken, CancellationToken token = default)
    {
        var isGameOngoing = await _gameActor.ActorRef.Ask<bool>(
            new GameQueries.IsGameOngoing(gameToken),
            token
        );
        return isGameOngoing;
    }

    public async Task<string> StartGameAsync(
        string userId1,
        string userId2,
        TimeControlSettings timeControl,
        bool isRated
    )
    {
        var token = await _gameTokenGenerator.GenerateUniqueGameToken();
        var whitePlayer = await CreatePlayer(userId1, GameColor.White, timeControl);
        var blackPlayer = await CreatePlayer(userId2, GameColor.Black, timeControl);
        await _gameActor.ActorRef.Ask<GameResponses.GameStarted>(
            new GameCommands.StartGame(token, whitePlayer, blackPlayer, timeControl, isRated)
        );

        return token;
    }

    public async Task<ErrorOr<GameState>> GetGameStateAsync(
        string gameToken,
        string userId,
        CancellationToken token = default
    )
    {
        var response = await _gameActor.ActorRef.AskExpecting<GameResponses.GameStateResponse>(
            new GameQueries.GetGameState(gameToken, userId),
            token
        );
        if (response.IsError)
            return response.Errors;
        return response.Value.State;
    }

    public async Task<ErrorOr<GameResponses.GamePlayers>> GetGamePlayersAsync(
        string gameToken,
        CancellationToken token = default
    )
    {
        var response = await _gameActor.ActorRef.AskExpecting<GameResponses.GamePlayers>(
            new GameQueries.GetGamePlayers(gameToken),
            token
        );
        return response;
    }

    public async Task<ErrorOr<Success>> MakeMoveAsync(
        string gameToken,
        string userId,
        AlgebraicPoint from,
        AlgebraicPoint to,
        CancellationToken token = default
    )
    {
        var response = await _gameActor.ActorRef.AskExpecting<GameResponses.PieceMoved>(
            new GameCommands.MovePiece(gameToken, userId, from, to),
            token
        );
        if (response.IsError)
            return response.Errors;
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> EndGameAsync(
        string gameToken,
        string userId,
        CancellationToken token = default
    )
    {
        var response = await _gameActor.ActorRef.AskExpecting<GameResponses.GameEnded>(
            new GameCommands.EndGame(gameToken, userId),
            token
        );
        if (response.IsError)
            return response.Errors;
        return Result.Success;
    }

    private async Task<GamePlayer> CreatePlayer(
        string userId,
        GameColor color,
        TimeControlSettings timeControl
    )
    {
        var user = await _userManager.FindByIdAsync(userId);

        int? rating = null;
        if (user is not null)
            rating = await _ratingService.GetRatingAsync(
                user,
                _timeControlTranslator.FromSeconds(timeControl.BaseSeconds)
            );

        return new GamePlayer(
            UserId: userId,
            Color: color,
            UserName: user?.UserName ?? "Guest",
            CountryCode: user?.CountryCode,
            Rating: rating
        );
    }
}
