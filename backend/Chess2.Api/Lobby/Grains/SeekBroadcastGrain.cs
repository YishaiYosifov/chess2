using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.Users.Models;
using Orleans.Streams;

namespace Chess2.Api.Lobby.Grains;

[Alias("Chess2.Api.Lobby.Grains.IOpenSeekWatcherGrain")]
public interface IOpenSeekWatcherGrain : IGrainWithIntegerKey
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

public record OpenSeek(SeekKey SeekKey, string UserName, TimeControl TimeControl, int? Rating);

public class OpenSeekEntry()
{
    public required OpenSeek OpenSeek { get; init; }
    public required Seeker Seeker { get; init; }
    public required HashSet<string> SubscribedUserIds { get; init; }
}

[KeepAlive]
public class SeekBroadcastGrain(
    IOpenSeekNotifier openSeekNotifier,
    ITimeControlTranslator timeControlTranslator
) : Grain, IOpenSeekWatcherGrain
{
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
                return;

            watchingSeeks.Add(openSeek.OpenSeek);
            openSeek.SubscribedUserIds.Add(seeker.UserId);
            if (watchingSeeks.Count > 10)
                return;
        }

        await _openSeekNotifier.NotifyOpenSeekAsync([seeker.UserId], watchingSeeks);
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
        if (!_openSeeks.TryGetValue(@event.SeekKey, out var openSeek))
            return;

        await _openSeekNotifier.NotifyOpenSeekEndedAsync(
            openSeek.SubscribedUserIds,
            @event.SeekKey
        );
        _openSeeks.Remove(@event.SeekKey);
    }

    private async Task OnSeekCreated(OpenSeekCreatedEvent @event, StreamSequenceToken _)
    {
        var openSeek = RegisterOpenSeeker(@event.Seeker, @event.SeekKey);
        await _openSeekNotifier.NotifyOpenSeekAsync(
            openSeek.SubscribedUserIds,
            [openSeek.OpenSeek]
        );
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
                RegisterOpenSeeker(seeker, new(seeker.UserId, pool));
            }
        }
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
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
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromMinutes(10)
        );

        await base.OnActivateAsync(cancellationToken);
    }

    private OpenSeekEntry RegisterOpenSeeker(Seeker seeker, SeekKey seekKey)
    {
        int? rating = seeker is RatedSeeker ratedSeeker ? ratedSeeker.Rating.Value : null;
        var timeControl = _timeControlTranslator.FromSeconds(seekKey.Pool.TimeControl.BaseSeconds);

        HashSet<string> matchingUserIds = [];
        foreach (var (userId, watcher) in _connections)
        {
            if (watcher.Seeker.IsCompatibleWith(seeker) && seeker.IsCompatibleWith(watcher.Seeker))
            {
                matchingUserIds.Add(userId);
            }
        }

        OpenSeek openSeek = new(seekKey, seeker.UserName, timeControl, rating);
        OpenSeekEntry entry = new()
        {
            OpenSeek = openSeek,
            Seeker = seeker,
            SubscribedUserIds = matchingUserIds,
        };

        _openSeeks.TryAdd(seekKey, entry);
        return entry;
    }
}
