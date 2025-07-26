using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Extensions;
using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Actors;

public record Chatter(string ConnectionId, string UserName, bool IsPlaying);

public class GameChatActor : ReceiveActor
{
    private readonly string _gameToken;
    private readonly IServiceProvider _sp;
    private readonly IGameChatNotifier _gameChatNotifier;
    private readonly ChatSettings _settings;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private readonly Dictionary<string, Chatter> _chatters = [];

    public GameChatActor(
        string gameToken,
        IServiceProvider sp,
        IOptions<AppSettings> settings,
        IGameChatNotifier gameChatNotifier
    )
    {
        _gameToken = gameToken;
        _sp = sp;
        _gameChatNotifier = gameChatNotifier;
        _settings = settings.Value.Game.Chat;

        Receive<GameChatCommands.JoinChat>(HandleJoinChat);
        Receive<GameChatCommands.LeaveChat>(HandleLeaveChat);
        Receive<GameChatCommands.SendMessage>(HandleSendMessage);
        Receive<ReceiveTimeout>(_ => HandleTimeout());
    }

    private void HandleJoinChat(GameChatCommands.JoinChat joinGame)
    {
        if (_chatters.ContainsKey(joinGame.UserId))
        {
            _logger.Warning("User {0} already in chat for game {1}", joinGame.UserId, _gameToken);
            Sender.ReplyWithError(GameChatErrors.UserAlreadyJoined);
            return;
        }

        RunTask(async () =>
        {
            await using var scope = _sp.CreateAsyncScope();
            var liveGameService = scope.ServiceProvider.GetRequiredService<ILiveGameService>();
            var playersResult = await liveGameService.GetGamePlayersAsync(_gameToken);
            if (playersResult.IsError)
            {
                Sender.ReplyWithError(playersResult.Errors);
                return;
            }

            var isPlaying =
                joinGame.UserId == playersResult.Value.WhitePlayer.UserId
                || joinGame.UserId == playersResult.Value.BlackPlayer.UserId;
            _chatters[joinGame.UserId] = new Chatter(
                joinGame.ConnectionId,
                joinGame.UserName,
                isPlaying
            );
            await _gameChatNotifier.JoinChatAsync(_gameToken, joinGame.ConnectionId, isPlaying);

            _logger.Info("User {0} joined chat for game {1}", joinGame.UserId, _gameToken);

            Sender.Tell(new GameChatEvents.UserJoined());
        });
    }

    private void HandleLeaveChat(GameChatCommands.LeaveChat leaveChat)
    {
        if (!_chatters.Remove(leaveChat.UserId, out var chatter))
        {
            _logger.Warning(
                "User {0} not found in chat for game {1} when trying to leave",
                leaveChat.UserId,
                _gameToken
            );
            Sender.ReplyWithError(GameChatErrors.UserNotInChat);
            return;
        }

        RunTask(async () =>
        {
            await _gameChatNotifier.LeaveChatAsync(
                _gameToken,
                chatter.ConnectionId,
                chatter.IsPlaying
            );

            _logger.Info("User {0} left chat for game {1}", leaveChat.UserId, _gameToken);
            Sender.Tell(new GameChatEvents.UserLeft());
        });
    }

    private void HandleSendMessage(GameChatCommands.SendMessage sendMessage)
    {
        if (string.IsNullOrWhiteSpace(sendMessage.Message))
        {
            _logger.Warning(
                "Empty message from user {0} for game {1}",
                sendMessage.UserId,
                _gameToken
            );
            Sender.ReplyWithError(GameChatErrors.InvalidMessage);
            return;
        }

        if (sendMessage.Message.Length > _settings.MaxMessageLength)
        {
            _logger.Warning(
                "Message from user {0} exceeds max length for game {1}",
                sendMessage.UserId,
                _gameToken
            );
            Sender.ReplyWithError(GameChatErrors.InvalidMessage);
            return;
        }

        if (!_chatters.TryGetValue(sendMessage.UserId, out var chatter))
        {
            _logger.Warning(
                "User {0} not found in chat for game {1} when trying to send message",
                sendMessage.UserId,
                _gameToken
            );
            Sender.ReplyWithError(GameChatErrors.UserNotInChat);
            return;
        }

        RunTask(
            () =>
                _gameChatNotifier.SendMessageAsync(
                    _gameToken,
                    chatter.UserName,
                    sendMessage.Message,
                    chatter.IsPlaying
                )
        );
        Sender.Tell(new GameChatEvents.MessageSent());
    }

    private void HandleTimeout()
    {
        Context.Parent.Tell(new Passivate(PoisonPill.Instance));
        _logger.Info("No chat recent chat messages in game {0}, passivating actor", _gameToken);
    }

    protected override void PreStart()
    {
        Context.SetReceiveTimeout(TimeSpan.FromSeconds(120));
        base.PreStart();
    }
}
