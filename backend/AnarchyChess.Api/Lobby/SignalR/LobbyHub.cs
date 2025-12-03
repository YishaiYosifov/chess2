using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Infrastructure.SignalR;
using AnarchyChess.Api.Lobby.Grains;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Matchmaking.Services;
using AnarchyChess.Api.Profile.Models;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace AnarchyChess.Api.Lobby.SignalR;

public interface ILobbyHubClient : IAnarchyChessHubClient
{
    Task MatchFoundAsync(string token);
    Task SeekFailedAsync(PoolKey pool);

    Task ReceiveOngoingGamesAsync(IEnumerable<OngoingGame> games);
    Task OngoingGameEndedAsync(GameToken gameToken);
}

[Authorize(AuthPolicies.ActiveSession)]
public class LobbyHub(
    ILogger<LobbyHub> logger,
    ISeekerCreator seekerCreator,
    IGrainFactory grains,
    IAuthService authService,
    IValidator<TimeControlSettingsRequest> timeControlValidator,
    ILobbyNotifier lobbyNotifier
) : AnarchyChessHub<ILobbyHubClient>
{
    private readonly ILogger<LobbyHub> _logger = logger;
    private readonly ISeekerCreator _seekerCreator = seekerCreator;
    private readonly IGrainFactory _grains = grains;
    private readonly IAuthService _authService = authService;
    private readonly IValidator<TimeControlSettingsRequest> _timeControlValidator =
        timeControlValidator;
    private readonly ILobbyNotifier _lobbyNotifier = lobbyNotifier;

    public async Task SeekRatedAsync(TimeControlSettingsRequest timeControlRequest)
    {
        var validationResult = _timeControlValidator.Validate(timeControlRequest);
        if (!validationResult.IsValid)
        {
            await HandleErrors(validationResult.Errors.ToErrorList());
            return;
        }

        var userResult = await _authService.GetLoggedInUserAsync(Context.User);
        if (userResult.IsError)
        {
            await HandleErrors(userResult.Errors);
            return;
        }

        var user = userResult.Value;
        _logger.LogInformation("User {UserId} seeking rated match", user.Id);

        TimeControlSettings timeControl = new(timeControlRequest);
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

    public async Task SeekCasualAsync(TimeControlSettingsRequest timeControlRequest)
    {
        var validationResult = _timeControlValidator.Validate(timeControlRequest);
        if (!validationResult.IsValid)
        {
            await HandleErrors(validationResult.Errors.ToErrorList());
            return;
        }

        var seekerResult = await _authService.MatchAuthTypeAsync<Seeker>(
            Context.User,
            whenAuthed: async user => await _seekerCreator.CreateAuthedCasualSeekerAsync(user),
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
            new PoolKey(PoolType.Casual, new TimeControlSettings(timeControlRequest))
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

    public async Task CleanupConnectionAsync()
    {
        if (!TryGetUserId(out var userId))
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        _logger.LogInformation("User {UserId} cleaning up their seeks", userId);
        var grain = _grains.GetGrain<IPlayerSessionGrain>(userId);
        await grain.CleanupConnectionAsync(Context.ConnectionId);
    }

    public async Task MatchWithOpenSeekAsync(UserId matchWith, PoolKey pool)
    {
        var seekerResult = await _authService.MatchAuthTypeAsync<Seeker>(
            Context.User,
            whenAuthed: async user => await _seekerCreator.CreateRatedOpenSeekerAsync(user),
            whenGuest: userId =>
                Task.FromResult<Seeker>(_seekerCreator.CreateGuestCasualSeeker(userId))
        );
        if (seekerResult.IsError)
        {
            await HandleErrors(seekerResult.Errors);
            return;
        }
        var seeker = seekerResult.Value;

        _logger.LogInformation(
            "User {UserId} trying to match with open seek of {MatchWith}",
            seeker.UserId,
            matchWith
        );

        var grain = _grains.GetGrain<IPlayerSessionGrain>(seeker.UserId);
        var matchResult = await grain.MatchWithOpenSeekAsync(
            Context.ConnectionId,
            seeker,
            matchWith,
            pool
        );
        if (matchResult.IsError)
            await HandleErrors(matchResult.Errors);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        if (!TryGetUserId(out var userId))
        {
            _logger.LogWarning("User connected to lobby hub without a user ID");
            return;
        }

        await NotifyOngoingGames(userId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

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

    private async Task NotifyOngoingGames(UserId userId)
    {
        var playerSessionGrain = _grains.GetGrain<IPlayerSessionGrain>(userId);
        var ongoingGames = await playerSessionGrain.GetOngoingGamesAsync();
        if (ongoingGames.Count > 0)
            await _lobbyNotifier.NotifyOngoingGamesAsync(Context.ConnectionId, ongoingGames);
    }
}
