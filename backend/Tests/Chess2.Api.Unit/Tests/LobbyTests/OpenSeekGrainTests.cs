using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Lobby.Grains;
using Chess2.Api.Lobby.Models;
using Chess2.Api.Lobby.Services;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Stream;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace Chess2.Api.Unit.Tests.LobbyTests;

public class OpenSeekGrainTests : BaseGrainTest
{
    private readonly IOpenSeekNotifier _openSeekNotifierMock = Substitute.For<IOpenSeekNotifier>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IPoolDirectoryGrain _poolDirectoryGrainMock =
        Substitute.For<IPoolDirectoryGrain>();

    private readonly DateTime _fakeNow;

    private readonly PoolKey _ratedPoolKey = new(
        PoolType.Rated,
        new TimeControlSettings(BaseSeconds: 600, IncrementSeconds: 30)
    );
    private readonly TimeControl _ratedPoolTimeControl = TimeControl.Rapid;

    private readonly PoolKey _casualPoolKey = new(
        PoolType.Casual,
        new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 20)
    );
    private readonly TimeControl _casualPoolTimeControl = TimeControl.Blitz;

    public OpenSeekGrainTests()
    {
        _fakeNow = DateTime.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        _poolDirectoryGrainMock.GetAllSeekersAsync().Returns([]);
        Silo.AddProbe(_ => _poolDirectoryGrainMock);

        Silo.ServiceProvider.AddService(_openSeekNotifierMock);
        Silo.ServiceProvider.AddService(_timeProviderMock);
        Silo.ServiceProvider.AddService<ITimeControlTranslator>(new TimeControlTranslator());
    }

    private TestStream<OpenSeekCreatedEvent> ProbeOpenSeekCreatedStream() =>
        Silo.AddStreamProbe<OpenSeekCreatedEvent>(
            MatchmakingStreamConstants.OpenSeekCreatedStream,
            streamNamespace: null,
            Streaming.StreamProvider
        );

    private TestStream<OpenSeekRemovedEvent> ProbeOpenSeekRemovedStream() =>
        Silo.AddStreamProbe<OpenSeekRemovedEvent>(
            MatchmakingStreamConstants.OpenSeekRemovedStream,
            streamNamespace: null,
            Streaming.StreamProvider
        );

    [Fact]
    public async Task SubscribeAsync_notifies_of_all_compatible_seeks()
    {
        var createStream = ProbeOpenSeekCreatedStream();
        var seeker = new CasualSeekerFaker().Generate();
        var matchSeeker = new CasualSeekerFaker().Generate();
        var unmatchSeeker = new RatedSeekerFaker().Generate();
        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);
        await createStream.OnNextBatchAsync(
            [
                new(new(matchSeeker.UserId, _casualPoolKey), matchSeeker),
                new(new(unmatchSeeker.UserId, _ratedPoolKey), unmatchSeeker),
            ]
        );
        _openSeekNotifierMock.ClearReceivedCalls();

        await grain.SubscribeAsync("conn", seeker);

        List<OpenSeek> expectedSeeks =
        [
            new OpenSeek(
                new(matchSeeker.UserId, _casualPoolKey),
                UserName: matchSeeker.UserName,
                TimeControl: _casualPoolTimeControl,
                Rating: null
            ),
        ];
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<ConnectionId>("conn"),
                Arg.Is<IEnumerable<OpenSeek>>(x => x.SequenceEqual(expectedSeeks))
            );
    }

    [Fact]
    public async Task SubscribeAsync_limits_to_10_seeks()
    {
        var createStream = ProbeOpenSeekCreatedStream();
        var seeker = new CasualSeekerFaker().Generate();
        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        var compatibleSeeks = Enumerable
            .Range(0, 15)
            .Select(i =>
            {
                var s = new CasualSeekerFaker().Generate();
                return new OpenSeekCreatedEvent(new(s.UserId, _casualPoolKey), s);
            })
            .ToList();
        await createStream.OnNextBatchAsync(compatibleSeeks);
        _openSeekNotifierMock.ClearReceivedCalls();

        await grain.SubscribeAsync("conn", seeker);

        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<ConnectionId>("conn"),
                Arg.Is<IEnumerable<OpenSeek>>(x => x.Count() == 10)
            );
    }

    [Fact]
    public async Task UnsubscribeAsync_removes_user_and_stops_notifications()
    {
        var createStream = ProbeOpenSeekCreatedStream();
        var seeker = new CasualSeekerFaker().Generate();
        var matchSeeker = new CasualSeekerFaker().Generate();
        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        await grain.SubscribeAsync("conn1", seeker);

        await grain.UnsubscribeAsync(seeker.UserId, "conn1");

        // push a compatible seek after unsubscribe
        await createStream.OnNextAsync(
            new OpenSeekCreatedEvent(new(matchSeeker.UserId, _casualPoolKey), matchSeeker)
        );

        await _openSeekNotifierMock
            .Received(0)
            .NotifyOpenSeekAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<OpenSeek>>());
    }

    [Fact]
    public async Task UnsubscribeAsync_removes_only_the_specified_connection_when_multiple_exist()
    {
        var createStream = ProbeOpenSeekCreatedStream();
        var seeker = new CasualSeekerFaker().Generate();
        var matchSeeker = new CasualSeekerFaker().Generate();
        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        await grain.SubscribeAsync("conn1", seeker);
        await grain.SubscribeAsync("conn2", seeker);

        await grain.UnsubscribeAsync(seeker.UserId, "conn1");

        await createStream.OnNextAsync(
            new OpenSeekCreatedEvent(new(matchSeeker.UserId, _casualPoolKey), matchSeeker)
        );

        List<string> expectedIds = [seeker.UserId];
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(expectedIds)),
                Arg.Is<IEnumerable<OpenSeek>>(seeks =>
                    seeks.Any(s => s.SeekKey.UserId == matchSeeker.UserId)
                )
            );
    }

    [Fact]
    public async Task SeekCreated_event_notifies_all_compatible_watchers()
    {
        var createStream = ProbeOpenSeekCreatedStream();
        var seeker1 = new CasualSeekerFaker().Generate();
        var seeker2 = new CasualSeekerFaker().Generate();
        var incompatibleSeeker = new RatedSeekerFaker().Generate();
        var matchSeeker = new CasualSeekerFaker().Generate();

        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        await grain.SubscribeAsync("conn1", seeker1);
        await grain.SubscribeAsync("conn2", seeker2);
        await grain.SubscribeAsync("conn3", incompatibleSeeker);

        await createStream.OnNextAsync(
            new OpenSeekCreatedEvent(new(matchSeeker.UserId, _casualPoolKey), matchSeeker)
        );

        List<string> expectedUserIds = [seeker1.UserId, seeker2.UserId];
        List<OpenSeek> expectedSeeks =
        [
            new OpenSeek(
                new(matchSeeker.UserId, _casualPoolKey),
                matchSeeker.UserName,
                _casualPoolTimeControl,
                Rating: null
            ),
        ];
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(expectedUserIds)),
                Arg.Is<IEnumerable<OpenSeek>>(seeks => seeks.SequenceEqual(expectedSeeks))
            );
    }

    [Fact]
    public async Task SeekEnded_event_notifies_all_compatible_watchers()
    {
        var createStream = ProbeOpenSeekCreatedStream();
        var removeStream = ProbeOpenSeekRemovedStream();

        var seeker1 = new CasualSeekerFaker().Generate();
        var seeker2 = new CasualSeekerFaker().Generate();
        var incompatibleSeeker = new RatedSeekerFaker().Generate();
        var matchSeeker = new CasualSeekerFaker().Generate();

        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        await grain.SubscribeAsync("conn1", seeker1);
        await grain.SubscribeAsync("conn2", seeker2);
        await grain.SubscribeAsync("conn3", incompatibleSeeker);

        await createStream.OnNextAsync(
            new OpenSeekCreatedEvent(new(matchSeeker.UserId, _casualPoolKey), matchSeeker)
        );
        await removeStream.OnNextAsync(
            new OpenSeekRemovedEvent(new(matchSeeker.UserId, _casualPoolKey))
        );

        List<string> expectedUserIds = [seeker1.UserId, seeker2.UserId];
        SeekKey expectedSeekKey = new(matchSeeker.UserId, _casualPoolKey);
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekEndedAsync(
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(expectedUserIds)),
                Arg.Is<SeekKey>(key => key.Equals(expectedSeekKey))
            );
    }

    [Fact]
    public async Task CleanStaleConnectionsAsync_removes_stale_connections_and_keeps_fresh()
    {
        var now = DateTimeOffset.UtcNow;
        var timeProviderMock = Substitute.For<TimeProvider>();
        timeProviderMock.GetUtcNow().Returns(now);

        var freshSeeker = new CasualSeekerFaker()
            .RuleFor(s => s.CreatedAt, now.AddMinutes(-4))
            .Generate();

        var staleSeeker = new CasualSeekerFaker()
            .RuleFor(s => s.CreatedAt, now.AddMinutes(-6))
            .Generate();

        var matchSeeker = new CasualSeekerFaker().Generate();

        var createStream = ProbeOpenSeekCreatedStream();
        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        await grain.SubscribeAsync("freshConn", freshSeeker);
        await grain.SubscribeAsync("staleConn", staleSeeker);

        await Silo.FireTimerAsync(OpenSeekGrain.StaleTimer);

        // add a new compatible seek to make sure only fresh conn receives it
        await createStream.OnNextAsync(
            new OpenSeekCreatedEvent(new(matchSeeker.UserId, _casualPoolKey), matchSeeker)
        );

        List<string> expectedIds = [freshSeeker.UserId];
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(expectedIds)),
                Arg.Any<IEnumerable<OpenSeek>>()
            );
    }

    [Fact]
    public async Task RefetchSeeksAsync_notifies_all_compatible_watchers_after_fetch()
    {
        var createStream = ProbeOpenSeekCreatedStream();

        var casualWatcher = new CasualSeekerFaker().Generate();
        var ratedWatcher = new OpenRatedSeekerFaker()
            .RuleFor(
                x => x.Ratings,
                new Dictionary<TimeControl, int> { [_ratedPoolTimeControl] = 1500 }
            )
            .Generate();

        var casualSeeker = new CasualSeekerFaker().Generate();
        var ratedSeeker = new RatedSeekerFaker()
            .RuleFor(x => x.Rating, new SeekerRatingFaker(1500, _ratedPoolTimeControl))
            .Generate();

        var grain = await Silo.CreateGrainAsync<OpenSeekGrain>(0);

        _poolDirectoryGrainMock
            .GetAllSeekersAsync()
            .Returns(
                new Dictionary<PoolKey, List<Seeker>>
                {
                    [_casualPoolKey] = [casualSeeker],
                    [_ratedPoolKey] = [ratedSeeker],
                }
            );

        await Silo.FireTimerAsync(OpenSeekGrain.RefetchTimer);

        await grain.SubscribeAsync("casualconn", casualWatcher);
        await grain.SubscribeAsync("ratedconn", ratedWatcher);
        List<OpenSeek> expectedCasualSeeks =
        [
            new OpenSeek(
                new(casualSeeker.UserId, _casualPoolKey),
                casualSeeker.UserName,
                _casualPoolTimeControl,
                null
            ),
        ];
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<ConnectionId>("casualconn"),
                Arg.Is<IEnumerable<OpenSeek>>(x => x.SequenceEqual(expectedCasualSeeks))
            );

        List<OpenSeek> expectedRatedSeeks =
        [
            .. expectedCasualSeeks,
            new OpenSeek(
                new(ratedSeeker.UserId, _ratedPoolKey),
                ratedSeeker.UserName,
                _ratedPoolTimeControl,
                Rating: ratedSeeker.Rating.Value
            ),
        ];
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<ConnectionId>("ratedconn"),
                Arg.Is<IEnumerable<OpenSeek>>(x => x.SequenceEqual(expectedRatedSeeks))
            );
    }
}
