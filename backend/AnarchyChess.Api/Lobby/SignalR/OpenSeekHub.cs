using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.Sharding;
using AnarchyChess.Api.Infrastructure.SignalR;
using AnarchyChess.Api.Lobby.Grains;
using AnarchyChess.Api.Lobby.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Matchmaking.Services;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Lobby.SignalR;

public interface IOpenSeekHubClient : IAnarchyChessHubClient
{
    Task NewOpenSeeksAsync(IEnumerable<OpenSeek> openSeeks);
    Task OpenSeekEndedAsync(UserId seekerId, PoolKey pool);
}

[Authorize(AuthPolicies.ActiveSession)]
public class OpenSeekHub(
    ILogger<OpenSeekHub> logger,
    ISeekerCreator seekerCreator,
    IAuthService authService,
    IGrainFactory grains,
    IOptions<AppSettings> settings,
    IShardRouter shardRouter
) : AnarchyChessHub<IOpenSeekHubClient>
{
    private readonly ILogger<OpenSeekHub> _logger = logger;
    private readonly ISeekerCreator _seekerCreator = seekerCreator;
    private readonly IAuthService _authService = authService;
    private readonly IGrainFactory _grains = grains;
    private readonly IShardRouter _shardRouter = shardRouter;
    private readonly LobbySettings _settings = settings.Value.Lobby;

    public async Task SubscribeAsync()
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
            "User {UserId} subscribing to open seeks with connection {ConnectionId}",
            seeker.UserId,
            Context.ConnectionId
        );

        var shard = _shardRouter.GetShardNumber(seeker.UserId, _settings.OpenSeekShardCount);
        var grain = _grains.GetGrain<IOpenSeekGrain>(shard);
        await grain.SubscribeAsync(Context.ConnectionId, seeker);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning(
                    "User disconnected from open seek hub without a user ID, cannot unsubscribe"
                );
                return;
            }
            _logger.LogInformation(
                "User {UserId} with connection {ConnectionId} disconnected from open seeks",
                userId,
                Context.ConnectionId
            );
            var shard = _shardRouter.GetShardNumber(userId, _settings.OpenSeekShardCount);
            var seekWatcherGrain = _grains.GetGrain<IOpenSeekGrain>(shard);
            await seekWatcherGrain.UnsubscribeAsync(userId, Context.ConnectionId);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
