using System.Security.Claims;
using Akka.Hosting;
using Chess2.Api.Auth.Services;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.Shared.Extensions;
using ErrorOr;

namespace Chess2.Api.LiveGame.Services;

public interface IGameChatService
{
    Task<ErrorOr<Success>> JoinChat(
        string gameToken,
        string connectionId,
        ClaimsPrincipal? userClaims,
        CancellationToken token = default
    );
    Task<ErrorOr<Success>> LeaveChat(
        string gameToken,
        string userId,
        CancellationToken token = default
    );
    Task<ErrorOr<Success>> SendMessage(
        string gameToken,
        string userId,
        string message,
        CancellationToken token = default
    );
}

public class GameChatService(IAuthService authService, IRequiredActor<GameChatActor> gameChatActor)
    : IGameChatService
{
    private readonly IAuthService _authService = authService;
    private readonly IRequiredActor<GameChatActor> _gameChatActor = gameChatActor;

    public async Task<ErrorOr<Success>> JoinChat(
        string gameToken,
        string connectionId,
        ClaimsPrincipal? userClaims,
        CancellationToken token = default
    )
    {
        var userResult = await _authService.GetLoggedInUserAsync(userClaims);
        if (userResult.IsError)
            return userResult.Errors;
        var user = userResult.Value;

        var result = await _gameChatActor.ActorRef.AskExpecting<GameChatEvents.UserJoined>(
            new GameChatCommands.JoinChat(
                gameToken,
                connectionId,
                user.Id,
                user.UserName ?? "Unknown"
            ),
            token
        );
        if (result.IsError)
            return result.Errors;

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> LeaveChat(
        string gameToken,
        string userId,
        CancellationToken token = default
    )
    {
        var result = await _gameChatActor.ActorRef.AskExpecting<GameChatEvents.UserLeft>(
            new GameChatCommands.LeaveChat(gameToken, userId),
            token
        );
        if (result.IsError)
            return result.Errors;
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> SendMessage(
        string gameToken,
        string userId,
        string message,
        CancellationToken token = default
    )
    {
        var result = await _gameChatActor.ActorRef.AskExpecting<GameChatEvents.MessageSent>(
            new GameChatCommands.SendMessage(gameToken, userId, message),
            token
        );
        if (result.IsError)
            return result.Errors;
        return Result.Success;
    }
}
