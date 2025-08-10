using Chess2.Api.Auth.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.PlayerSession.Grains;
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
    ISeekerCreator seekerCreator,
    IGrainFactory grains,
    IAuthService authService
) : Chess2Hub<IMatchmakingHubClient>
{
    private readonly ILogger<LobbyHub> _logger = logger;
    private readonly ISeekerCreator _seekerCreator = seekerCreator;
    private readonly IGrainFactory _grains = grains;
    private readonly IAuthService _authService = authService;

    public async Task SeekRatedAsync(TimeControlSettings timeControl)
    {
        var userResult = await _authService.GetLoggedInUserAsync(Context.User);
        if (userResult.IsError)
        {
            await HandleErrors(userResult.Errors);
            return;
        }

        var user = userResult.Value;
        _logger.LogInformation("User {UserId} seeking rated match", user.Id);

        var seeker = await _seekerCreator.RatedSeekerAsync(user, timeControl);
        var grain = _grains.GetGrain<IPlayerSessionGrain>(user.Id);
        await grain.CreateSeekAsync(Context.ConnectionId, seeker, new(PoolType.Rated, timeControl));
    }

    public async Task SeekCasualAsync(TimeControlSettings timeControl)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var userResult = await _authService.GetLoggedInUserAsync(Context.User);
        var user = userResult.IsError ? null : userResult.Value;

        _logger.LogInformation("User {UserId} seeking casual match", userId);

        var seeker = user is null
            ? _seekerCreator.CasualSeeker(userId)
            : _seekerCreator.CasualSeeker(user);
        var grain = _grains.GetGrain<IPlayerSessionGrain>(userId);
        await grain.CreateSeekAsync(
            Context.ConnectionId,
            seeker,
            new(PoolType.Casual, timeControl)
        );
    }

    public async Task CancelSeekAsync()
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        _logger.LogInformation("User {UserId} cancelled their seek", userId);
        var grain = _grains.GetGrain<IPlayerSessionGrain>(userId);
        await grain.CancelSeekAsync(Context.ConnectionId);
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
            var grain = _grains.GetGrain<IPlayerSessionGrain>(userId);
            await grain.CancelSeekAsync(Context.ConnectionId);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
