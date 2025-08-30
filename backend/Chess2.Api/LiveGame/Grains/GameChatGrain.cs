using System.Collections.Concurrent;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.Profile.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Grains;

[Alias("Chess2.Api.LiveGame.Grains.IGameChatGrain")]
public interface IGameChatGrain : IGrainWithStringKey
{
    [Alias("JoinChatAsync")]
    Task JoinChatAsync(string connectionId, string userId, CancellationToken token = default);

    [Alias("LeaveChatAsync")]
    Task LeaveChatAsync(string connectionId, string userId, CancellationToken token = default);

    [Alias("SendMessageAsync")]
    Task<ErrorOr<Success>> SendMessageAsync(
        string connectionId,
        string userId,
        string message,
        CancellationToken token = default
    );
}

public class GameChatGrain : Grain, IGameChatGrain, IGrainBase
{
    private readonly string _gameToken;

    private readonly ILogger<GameChatGrain> _logger;
    private readonly IGameChatNotifier _gameChatNotifier;
    private readonly IChatRateLimiter _chatRateLimiter;
    private readonly ChatSettings _settings;
    private readonly UserManager<AuthedUser> _userManager;
    private readonly IChatMessageLogger _chatMessageLogger;

    private PlayerRoster? _players;
    private readonly ConcurrentDictionary<string, string> _usernameCache = [];

    public GameChatGrain(
        ILogger<GameChatGrain> logger,
        UserManager<AuthedUser> userManager,
        IChatMessageLogger chatMessageLogger,
        IOptions<AppSettings> settings,
        IGameChatNotifier gameChatNotifier,
        IChatRateLimiter chatRateLimiter
    )
    {
        _gameToken = this.GetPrimaryKeyString();

        _logger = logger;
        _userManager = userManager;
        _chatMessageLogger = chatMessageLogger;
        _gameChatNotifier = gameChatNotifier;
        _chatRateLimiter = chatRateLimiter;
        _settings = settings.Value.Game.Chat;
    }

    public async Task JoinChatAsync(
        string connectionId,
        string userId,
        CancellationToken token = default
    )
    {
        var isPlaying = await IsUserPlayingAsync(userId);

        await _gameChatNotifier.JoinChatAsync(_gameToken, connectionId, isPlaying, token);
        _logger.LogInformation(
            "User {UserId} joined chat for game {GameToken}",
            userId,
            _gameToken
        );
    }

    public async Task LeaveChatAsync(
        string connectionId,
        string userId,
        CancellationToken token = default
    )
    {
        var isPlaying = await IsUserPlayingAsync(userId);
        await _gameChatNotifier.LeaveChatAsync(_gameToken, connectionId, isPlaying, token);

        _logger.LogInformation("User {UserId} left chat for game {GameToken}", userId, _gameToken);
    }

    public async Task<ErrorOr<Success>> SendMessageAsync(
        string connectionId,
        string userId,
        string message,
        CancellationToken token = default
    )
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogWarning(
                "Empty message from user {UserId} for game {GameToken}",
                userId,
                _gameToken
            );
            return GameChatErrors.InvalidMessage;
        }

        if (message.Length > _settings.MaxMessageLength)
        {
            _logger.LogWarning(
                "Message from user {UserId} exceeds max length for game {GameToken}",
                userId,
                _gameToken
            );
            return GameChatErrors.InvalidMessage;
        }

        if (!_chatRateLimiter.ShouldAllowRequest(userId, out var cooldownLeft))
            return GameChatErrors.OnCooldown;

        var usernameResult = await GetUsernameAsync(userId);
        if (usernameResult.IsError)
            return usernameResult.Errors;

        var isPlaying = await IsUserPlayingAsync(userId);
        await _gameChatNotifier.SendMessageAsync(
            _gameToken,
            usernameResult.Value,
            connectionId,
            cooldownLeft,
            message,
            isPlaying
        );
        await _chatMessageLogger.LogMessageAsync(_gameToken, userId, message, token);
        return Result.Success;
    }

    private async Task<ErrorOr<string>> GetUsernameAsync(string userId)
    {
        if (_usernameCache.TryGetValue(userId, out var username))
            return username;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return GameChatErrors.InvalidUser;

        username = user.UserName ?? "Unknown";
        _usernameCache.TryAdd(userId, username);
        return username;
    }

    private async Task<bool> IsUserPlayingAsync(string userId)
    {
        var playersResult = await GetPlayersAsync();
        if (playersResult.IsError)
            return false;

        var isPlaying =
            playersResult.Value.WhitePlayer.UserId == userId
            || playersResult.Value.BlackPlayer.UserId == userId;
        return isPlaying;
    }

    private async Task<ErrorOr<PlayerRoster>> GetPlayersAsync()
    {
        if (_players is not null)
            return _players;

        var playersResult = await GrainFactory.GetGrain<IGameGrain>(_gameToken).GetPlayersAsync();
        if (playersResult.IsError)
            return playersResult.Errors;

        _players = playersResult.Value;
        return _players;
    }
}
