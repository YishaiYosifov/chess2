using Chess2.Api.Auth.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.Matchmaking.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.Lobby.SignalR;

public interface IMatchmakingHubClient : IChess2HubClient
{
    public Task MatchFoundAsync(string token);
    public Task MatchFailedAsync();
}

[Authorize(AuthPolicies.AuthedSesssion)]
public class LobbyHub(
    ILogger<LobbyHub> logger,
    IMatchmakingService matchmakingService,
    IAuthService authService
) : Chess2Hub<IMatchmakingHubClient>
{
    private readonly IMatchmakingService _matchmakingService = matchmakingService;
    private readonly ILogger<LobbyHub> _logger = logger;
    private readonly IAuthService _authService = authService;

    public async Task SeekRatedAsync(TimeControlSettings timeControl)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var userResult = await _authService.GetLoggedInUserAsync(Context.User);
        if (userResult.IsError)
        {
            await HandleErrors(userResult.Errors);
            return;
        }
        _logger.LogInformation("User {UserId} seeking rated match", userId);
        await _matchmakingService.SeekRatedAsync(
            userResult.Value,
            Context.ConnectionId,
            timeControl
        );
    }

    public async Task SeekCasualAsync(TimeControlSettings timeControl)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        _logger.LogInformation("User {UserId} seeking casual match", userId);
        _matchmakingService.SeekCasual(userId, Context.ConnectionId, timeControl);
    }

    public async Task CancelSeekAsync()
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        _logger.LogInformation("User {UserId} cancelled their seek", userId);
        _matchmakingService.CancelSeek(userId, Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning(
                    "User disconnected from matchmaking hub without a user ID, cannot cancel seek"
                );
                return;
            }

            _logger.LogInformation(
                "User {UserId} disconnected from matchmaking hub, cancelling seek of connection of {ConnectionId} if it exists",
                userId,
                Context.ConnectionId
            );
            _matchmakingService.CancelSeek(userId, Context.ConnectionId);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
