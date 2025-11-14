using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Lobby.Models;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Orleans.Streams;

namespace Chess2.Api.Lobby.Grains;

[Alias("Chess2.Api.Lobby.Grains.IOpenSeekGrain")]
public interface IOpenSeekGrain : IGrainWithIntegerKey
{
    [Alias("Subscribe")]
    Task SubscribeAsync(ConnectionId connectionId, Seeker seeker);

    [Alias("Unsubscribe")]
    Task UnsubscribeAsync(UserId userId, ConnectionId connectionId);

    [Alias("InitializeAsync")]
    Task InitializeAsync();

#if DEBUG
    [Alias("ClearStateAsync")]
    Task ClearStateAsync();
#endif
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
    ILogger<OpenSeekGrain> logger,
    IOpenSeekNotifier openSeekNotifier,
    ITimeControlTranslator timeControlTranslator
) : Grain, IOpenSeekGrain
{
    public const int RefetchTimer = 0;

    private readonly ILogger<OpenSeekGrain> _logger = logger;
    private readonly IOpenSeekNotifier _openSeekNotifier = openSeekNotifier;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;

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

        _openSeeks.Remove(seekKey);
        await _openSeekNotifier.NotifyOpenSeekEndedAsync(
            openSeek.SubscribedUserIds,
            @event.UserId,
            @event.Pool
        );
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

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        var openSeekCreatedStream = streamProvider.GetStream<OpenSeekCreatedEvent>(
            nameof(OpenSeekCreatedEvent)
        );
        await openSeekCreatedStream.SubscribeAsync(OnSeekCreated);

        var openSeekRemovedStream = streamProvider.GetStream<OpenSeekRemovedEvent>(
            nameof(OpenSeekRemovedEvent)
        );
        await openSeekRemovedStream.SubscribeAsync(OnSeekEnded);

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

    public Task InitializeAsync() => Task.CompletedTask;

#if DEBUG
    public Task ClearStateAsync()
    {
        _connections.Clear();
        _openSeeks.Clear();
        return Task.CompletedTask;
    }
#endif
}
