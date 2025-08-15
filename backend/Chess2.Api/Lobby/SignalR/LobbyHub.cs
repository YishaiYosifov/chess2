using Chess2.Api.Auth.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.Lobby.SignalR;

public interface ILobbyHubClient : IChess2HubClient
{
    public Task MatchFoundAsync(string token);
    public Task MatchFailedAsync();
}

[Authorize(AuthPolicies.ActiveSession)]
public class LobbyHub(
    ILogger<LobbyHub> logger,
    ISeekerCreator seekerCreator,
    IGrainFactory grains,
    IAuthService authService
) : Chess2Hub<ILobbyHubClient>
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

        var seeker = await _seekerCreator.CreateRatedSeekerAsync(user, timeControl);
        var grain = _grains.GetGrain<IPlayerSessionGrain>(user.Id);
        var result = await grain.CreateSeekAsync(
            Context.ConnectionId,
            seeker,
            new PoolKey(PoolType.Rated, timeControl)
        );
        if (result.IsError)
            await HandleErrors(result.Errors);
    }

    public async Task SeekCasualAsync(TimeControlSettings timeControl)
    {
        var seekerResult = await _authService.MatchAuthTypeAsync(
            Context.User,
            whenAuthed: user =>
                Task.FromResult<Seeker>(_seekerCreator.CreateAuthedCasualSeeker(user)),
            whenGuest: userId =>
                Task.FromResult<Seeker>(_seekerCreator.CreateGuestCasualSeeker(userId))
        );
        if (seekerResult.IsError)
        {
            await HandleErrors(seekerResult.Errors);
            return;
        }
        var seeker = seekerResult.Value;

        _logger.LogInformation("User {UserId} seeking casual match", seeker.UserId);

        var grain = _grains.GetGrain<IPlayerSessionGrain>(seeker.UserId);
        var result = await grain.CreateSeekAsync(
            Context.ConnectionId,
            seeker,
            new(PoolType.Casual, timeControl)
        );
        if (result.IsError)
            await HandleErrors(result.Errors);
    }

    public async Task CancelSeekAsync(PoolKey pool)
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        _logger.LogInformation("User {UserId} cancelled their seek", userId);
        var grain = _grains.GetGrain<IPlayerSessionGrain>(userId);
        await grain.CancelSeekAsync(pool);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning(
                    "User disconnected from lobby hub without a user ID, cannot cancel seek"
                );
                return;
            }

            _logger.LogInformation(
                "User {UserId} disconnected from lobby hub, cancelling seek of connection of {ConnectionId} if it exists",
                userId,
                Context.ConnectionId
            );

            var playerSessionGrain = _grains.GetGrain<IPlayerSessionGrain>(userId);
            await playerSessionGrain.CleanupConnectionAsync(Context.ConnectionId);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
