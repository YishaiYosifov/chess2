using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Lobby.Models;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;
using Orleans.Streams;

namespace Chess2.Api.Lobby.Grains;

[Alias("Chess2.Api.Lobby.Grains.IOpenSeekGrain")]
public interface IOpenSeekGrain : IGrainWithIntegerKey
{
    [Alias("Subscribe")]
    Task SubscribeAsync(ConnectionId connectionId, Seeker seeker);

    [Alias("Unsubscribe")]
    Task UnsubscribeAsync(UserId userId, ConnectionId connectionId);
}

public class SeekWatcher
{
    public required HashSet<ConnectionId> ConnectionIds { get; init; }
    public required Seeker Seeker { get; init; }
}

public class OpenSeekEntry
{
    public required OpenSeek OpenSeek { get; init; }
    public required Seeker Seeker { get; init; }
    public required HashSet<string> SubscribedUserIds { get; init; }
}

public record SeekKey(UserId UserId, PoolKey Pool);

[KeepAlive]
public class OpenSeekGrain(
    IOpenSeekNotifier openSeekNotifier,
    ITimeControlTranslator timeControlTranslator,
    TimeProvider timeProvider
) : Grain, IOpenSeekGrain, IGrainBase
{
    public const int RefetchTimer = 0;
    public const int StaleTimer = 1;

    private readonly IOpenSeekNotifier _openSeekNotifier = openSeekNotifier;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly TimeProvider _timeProvider = timeProvider;

    private readonly Dictionary<UserId, SeekWatcher> _connections = [];
    private readonly Dictionary<SeekKey, OpenSeekEntry> _openSeeks = [];

    public async Task SubscribeAsync(ConnectionId connectionId, Seeker seeker)
    {
        if (_connections.TryGetValue(seeker.UserId, out var existingConnection))
        {
            existingConnection.ConnectionIds.Add(connectionId);
        }
        else
        {
            _connections[seeker.UserId] = new() { ConnectionIds = [connectionId], Seeker = seeker };
        }

        List<OpenSeek> watchingSeeks = [];
        foreach (var openSeek in _openSeeks.Values)
        {
            if (
                !seeker.IsCompatibleWith(openSeek.Seeker)
                || !openSeek.Seeker.IsCompatibleWith(seeker)
            )
                continue;

            watchingSeeks.Add(openSeek.OpenSeek);
            openSeek.SubscribedUserIds.Add(seeker.UserId);
            if (watchingSeeks.Count >= 10)
                break;
        }

        if (watchingSeeks.Count > 0)
        {
            await _openSeekNotifier.NotifyOpenSeekAsync(connectionId, watchingSeeks);
        }
    }

    public Task UnsubscribeAsync(UserId userId, ConnectionId connectionId)
    {
        if (!_connections.TryGetValue(userId, out var existingConnection))
            return Task.CompletedTask;

        existingConnection.ConnectionIds.Remove(connectionId);
        if (existingConnection.ConnectionIds.Count == 0)
        {
            _connections.Remove(userId);
        }
        return Task.CompletedTask;
    }

    private async Task OnSeekEnded(OpenSeekRemovedEvent @event, StreamSequenceToken _)
    {
        SeekKey seekKey = new(@event.UserId, @event.Pool);
        if (!_openSeeks.TryGetValue(seekKey, out var openSeek))
            return;

        await _openSeekNotifier.NotifyOpenSeekEndedAsync(
            openSeek.SubscribedUserIds,
            @event.UserId,
            @event.Pool
        );
        _openSeeks.Remove(seekKey);
    }

    private async Task OnSeekCreated(OpenSeekCreatedEvent @event, StreamSequenceToken _)
    {
        var openSeek = RegisterOpenSeeker(@event.Seeker, @event.Pool);
        if (openSeek.SubscribedUserIds.Count > 0)
        {
            await _openSeekNotifier.NotifyOpenSeekAsync(
                openSeek.SubscribedUserIds,
                [openSeek.OpenSeek]
            );
        }
    }

    private async Task RefetchSeeksAsync()
    {
        var poolDirectoryGrain = GrainFactory.GetGrain<IPoolDirectoryGrain>(0);
        var poolSeekers = await poolDirectoryGrain.GetAllSeekersAsync();

        _openSeeks.Clear();
        foreach (var (pool, seekers) in poolSeekers)
        {
            foreach (var seeker in seekers)
            {
                RegisterOpenSeeker(seeker, pool);
            }
        }
    }

    private Task CleanStaleConnectionsAsync()
    {
        var cutoff = _timeProvider.GetUtcNow() - TimeSpan.FromMinutes(5);
        var staleUserIds = _connections
            .Where(watcher => watcher.Value.Seeker.CreatedAt < cutoff)
            .Select(watcher => watcher.Key)
            .ToList();
        foreach (var userId in staleUserIds)
        {
            _connections.Remove(userId);
        }

        return Task.CompletedTask;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await RefetchSeeksAsync();

        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        var openSeekCreatedStream = streamProvider.GetStream<OpenSeekCreatedEvent>(
            MatchmakingStreamConstants.OpenSeekCreatedStream
        );
        await openSeekCreatedStream.SubscribeAsync(OnSeekCreated);

        var openSeekRemovedStream = streamProvider.GetStream<OpenSeekRemovedEvent>(
            MatchmakingStreamConstants.OpenSeekRemovedStream
        );
        await openSeekRemovedStream.SubscribeAsync(OnSeekEnded);

        this.RegisterGrainTimer(
            callback: RefetchSeeksAsync,
            dueTime: TimeSpan.FromMinutes(10),
            period: TimeSpan.FromMinutes(10)
        );
        this.RegisterGrainTimer(
            callback: CleanStaleConnectionsAsync,
            dueTime: TimeSpan.FromMinutes(5),
            period: TimeSpan.FromMinutes(5)
        );

        await base.OnActivateAsync(cancellationToken);
    }

    private OpenSeekEntry RegisterOpenSeeker(Seeker seeker, PoolKey pool)
    {
        int? rating = seeker is RatedSeeker ratedSeeker ? ratedSeeker.Rating.Value : null;
        var timeControl = _timeControlTranslator.FromSeconds(pool.TimeControl.BaseSeconds);

        HashSet<string> matchingUserIds = [];
        foreach (var (userId, watcher) in _connections)
        {
            if (watcher.Seeker.IsCompatibleWith(seeker) && seeker.IsCompatibleWith(watcher.Seeker))
            {
                matchingUserIds.Add(userId);
            }
        }

        OpenSeek openSeek = new(UserId: seeker.UserId, seeker.UserName, pool, timeControl, rating);
        OpenSeekEntry entry = new()
        {
            OpenSeek = openSeek,
            Seeker = seeker,
            SubscribedUserIds = matchingUserIds,
        };

        _openSeeks.TryAdd(new SeekKey(seeker.UserId, pool), entry);
        return entry;
    }
}
