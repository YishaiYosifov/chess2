using System.Collections.Concurrent;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Extensions;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Actors;

public class GameChatActor : ReceiveActor
{
    private readonly string _gameToken;
    private readonly IServiceProvider _sp;
    private readonly IGameChatNotifier _gameChatNotifier;
    private readonly IChatRateLimiter _chatRateLimiter;
    private readonly ChatSettings _settings;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private GameReplies.GamePlayers? _players;
    private readonly ConcurrentDictionary<string, string> _usernameCache = [];

    public GameChatActor(
        string gameToken,
        IServiceProvider sp,
        IOptions<AppSettings> settings,
        IGameChatNotifier gameChatNotifier,
        IChatRateLimiter chatRateLimiter
    )
    {
        _gameToken = gameToken;
        _sp = sp;
        _gameChatNotifier = gameChatNotifier;
        _chatRateLimiter = chatRateLimiter;
        _settings = settings.Value.Game.Chat;

        Receive<GameChatCommands.JoinChat>(HandleJoinChat);
        Receive<GameChatCommands.LeaveChat>(HandleLeaveChat);
        Receive<GameChatCommands.SendMessage>(HandleSendMessage);
        Receive<ReceiveTimeout>(_ => HandleTimeout());
    }

    private void HandleJoinChat(GameChatCommands.JoinChat joinGame)
    {
        RunTask(async () =>
        {
            var isPlayingResult = await IsUserPlayingAsync(joinGame.UserId);
            if (isPlayingResult.IsError)
            {
                Sender.ReplyWithError(isPlayingResult.Errors);
                return;
            }

            await _gameChatNotifier.JoinChatAsync(
                _gameToken,
                joinGame.ConnectionId,
                isPlayingResult.Value
            );
            _logger.Info("User {0} joined chat for game {1}", joinGame.UserId, _gameToken);

            Sender.Tell(new GameChatReplies.UserJoined());
        });
    }

    private void HandleLeaveChat(GameChatCommands.LeaveChat leaveChat)
    {
        RunTask(async () =>
        {
            var isPlayingResult = await IsUserPlayingAsync(leaveChat.UserId);
            if (isPlayingResult.IsError)
            {
                Sender.ReplyWithError(isPlayingResult.Errors);
                return;
            }

            await _gameChatNotifier.LeaveChatAsync(
                _gameToken,
                leaveChat.ConnectionId,
                isPlayingResult.Value
            );

            _logger.Info("User {0} left chat for game {1}", leaveChat.UserId, _gameToken);
            Sender.Tell(new GameChatReplies.UserLeft());
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

        if (!_chatRateLimiter.ShouldAllowRequest(sendMessage.UserId, out var cooldownLeft))
        {
            Sender.ReplyWithError(GameChatErrors.OnCooldown);
            return;
        }

        RunTask(async () =>
        {
            var isPlayingResult = await IsUserPlayingAsync(sendMessage.UserId);
            if (isPlayingResult.IsError)
            {
                Sender.ReplyWithError(isPlayingResult.Errors);
                return;
            }
            var usernameResult = await GetUsernameAsync(sendMessage.UserId);
            if (usernameResult.IsError)
            {
                Sender.ReplyWithError(usernameResult.Errors);
                return;
            }

            await _gameChatNotifier.SendMessageAsync(
                _gameToken,
                usernameResult.Value,
                sendMessage.ConnectionId,
                cooldownLeft,
                sendMessage.Message,
                isPlayingResult.Value
            );
            Sender.Tell(new GameChatReplies.MessageSent());
        });
        RunTask(() => LogMessageAsync(sendMessage.UserId, sendMessage.Message));
    }

    private async Task LogMessageAsync(string userId, string message)
    {
        await using var scope = _sp.CreateAsyncScope();
        var chatMessageService = scope.ServiceProvider.GetRequiredService<IChatMessageLogger>();

        await chatMessageService.LogMessageAsync(_gameToken, userId, message);
    }

    private async Task<ErrorOr<string>> GetUsernameAsync(string userId)
    {
        if (_usernameCache.TryGetValue(userId, out var username))
            return username;

        await using var scope = _sp.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>();
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return GameChatErrors.InvalidUser;

        username = user.UserName ?? "Unknown";
        _usernameCache.TryAdd(userId, username);
        return username;
    }

    private async Task<ErrorOr<bool>> IsUserPlayingAsync(string userId)
    {
        var playersResult = await GetPlayersAsync();
        if (playersResult.IsError)
            return false;

        var isPlaying =
            playersResult.Value.WhitePlayer.UserId == userId
            || playersResult.Value.BlackPlayer.UserId == userId;
        return isPlaying;
    }

    private async Task<ErrorOr<GameReplies.GamePlayers>> GetPlayersAsync()
    {
        if (_players is not null)
            return _players;

        await using var scope = _sp.CreateAsyncScope();
        var liveGameService = scope.ServiceProvider.GetRequiredService<ILiveGameService>();
        var playersResult = await liveGameService.GetGamePlayersAsync(_gameToken);

        _players = playersResult.Value;
        return playersResult;
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
