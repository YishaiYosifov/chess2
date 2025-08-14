using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.Sharding;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Lobby.SignalR;

public interface IOpenSeekHubClient : IChess2HubClient
{
    public Task NewOpenSeeksAsync(IEnumerable<OpenSeek> openSeeks);
    public Task OpenSeekEndedAsync(SeekKey seekKey);
}

[Authorize(AuthPolicies.ActiveSession)]
public class OpenSeekHub(
    ILogger<OpenSeekHub> logger,
    ISeekerCreator seekerCreator,
    IAuthService authService,
    IGrainFactory grains,
    IOptions<AppSettings> settings,
    IShardRouter shardRouter
) : Chess2Hub<IOpenSeekHubClient>
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
        _logger.LogInformation("User {UserId} subscribing to open seeks", seeker.UserId);

        var shard = _shardRouter.GetShardNumber(seeker.UserId, _settings.OpenSeekShardCount);
        var grain = _grains.GetGrain<IOpenSeekGrain>(shard);
        await grain.SubscribeAsync(Context.ConnectionId, seeker);
    }

    public override async Task OnConnectedAsync()
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

            var shard = _shardRouter.GetShardNumber(userId, _settings.OpenSeekShardCount);
            var seekWatcherGrain = _grains.GetGrain<IOpenSeekGrain>(shard);
            await seekWatcherGrain.UnsubscribeAsync(userId, Context.ConnectionId);
        }
        finally
        {
            await base.OnConnectedAsync();
        }
    }
}
