using Akka.Hosting;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.Shared.Extensions;
using ErrorOr;

namespace Chess2.Api.LiveGame.Services;

public interface IGameChatService
{
    Task<ErrorOr<Success>> JoinChat(
        string gameToken,
        string userId,
        string connectionId,
        CancellationToken token = default
    );
    Task<ErrorOr<Success>> LeaveChat(
        string gameToken,
        string userId,
        string connectionId,
        CancellationToken token = default
    );
    Task<ErrorOr<Success>> SendMessage(
        string gameToken,
        string userId,
        string connectionId,
        string message,
        CancellationToken token = default
    );
}

public class GameChatService(IRequiredActor<GameChatActor> gameChatActor) : IGameChatService
{
    private readonly IRequiredActor<GameChatActor> _gameChatActor = gameChatActor;

    public async Task<ErrorOr<Success>> JoinChat(
        string gameToken,
        string userId,
        string connectionId,
        CancellationToken token = default
    )
    {
        var result = await _gameChatActor.ActorRef.AskExpecting<GameChatEvents.UserJoined>(
            new GameChatCommands.JoinChat(
                GameToken: gameToken,
                UserId: userId,
                ConnectionId: connectionId
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
        string connectionId,
        CancellationToken token = default
    )
    {
        var result = await _gameChatActor.ActorRef.AskExpecting<GameChatEvents.UserLeft>(
            new GameChatCommands.LeaveChat(gameToken, connectionId, userId),
            token
        );
        if (result.IsError)
            return result.Errors;
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> SendMessage(
        string gameToken,
        string userId,
        string connectionId,
        string message,
        CancellationToken token = default
    )
    {
        var result = await _gameChatActor.ActorRef.AskExpecting<GameChatEvents.MessageSent>(
            new GameChatCommands.SendMessage(
                GameToken: gameToken,
                ConnectionId: connectionId,
                UserId: userId,
                Message: message
            ),
            token
        );
        if (result.IsError)
            return result.Errors;
        return Result.Success;
    }
}
