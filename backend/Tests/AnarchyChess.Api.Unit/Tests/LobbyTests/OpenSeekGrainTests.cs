using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.GameSnapshot.Services;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Lobby.Grains;
using AnarchyChess.Api.Lobby.Models;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Streams;

namespace AnarchyChess.Api.Unit.Tests.LobbyTests;

public class OpenSeekGrainTests : BaseGrainTest
{
    private readonly IOpenSeekNotifier _openSeekNotifierMock = Substitute.For<IOpenSeekNotifier>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly DateTime _fakeNow;

    private readonly PoolKey _ratedPoolKey = new(
        PoolType.Rated,
        new TimeControlSettings(BaseSeconds: 600, IncrementSeconds: 30)
    );

    private readonly PoolKey _casualPoolKey = new(
        PoolType.Casual,
        new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 20)
    );
    private readonly TimeControl _casualPoolTimeControl = TimeControl.Blitz;

    public OpenSeekGrainTests()
    {
        _fakeNow = DateTime.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        Silo.ServiceProvider.AddService(_openSeekNotifierMock);
        Silo.ServiceProvider.AddService(_timeProviderMock);
        Silo.ServiceProvider.AddService<ITimeControlTranslator>(new TimeControlTranslator());
    }

    private TestStream<OpenSeekCreatedEvent> ProbeOpenSeekCreatedStream() =>
        Silo.AddStreamProbe<OpenSeekCreatedEvent>(
            nameof(OpenSeekCreatedEvent),
            streamNamespace: null,
            Streaming.StreamProvider
        );

    private TestStream<OpenSeekRemovedEvent> ProbeOpenSeekRemovedStream() =>
        Silo.AddStreamProbe<OpenSeekRemovedEvent>(
            nameof(OpenSeekRemovedEvent),
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
            [new(matchSeeker, _casualPoolKey), new(unmatchSeeker, _ratedPoolKey)]
        );
        _openSeekNotifierMock.ClearReceivedCalls();

        await grain.SubscribeAsync("conn", seeker);

        List<OpenSeek> expectedSeeks =
        [
            new OpenSeek(
                matchSeeker.UserId,
                UserName: matchSeeker.UserName,
                _casualPoolKey,
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
                return new OpenSeekCreatedEvent(s, _casualPoolKey);
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
        await createStream.OnNextAsync(new OpenSeekCreatedEvent(matchSeeker, _casualPoolKey));

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

        await createStream.OnNextAsync(new OpenSeekCreatedEvent(matchSeeker, _casualPoolKey));

        List<string> expectedIds = [seeker.UserId];
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekAsync(
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(expectedIds)),
                Arg.Is<IEnumerable<OpenSeek>>(seeks =>
                    seeks.Any(s => s.UserId == matchSeeker.UserId)
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

        await createStream.OnNextAsync(new OpenSeekCreatedEvent(matchSeeker, _casualPoolKey));

        List<string> expectedUserIds = [seeker1.UserId, seeker2.UserId];
        List<OpenSeek> expectedSeeks =
        [
            new OpenSeek(
                matchSeeker.UserId,
                matchSeeker.UserName,
                _casualPoolKey,
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

        await createStream.OnNextAsync(new OpenSeekCreatedEvent(matchSeeker, _casualPoolKey));
        await removeStream.OnNextAsync(
            new OpenSeekRemovedEvent(matchSeeker.UserId, _casualPoolKey)
        );

        List<string> expectedUserIds = [seeker1.UserId, seeker2.UserId];
        SeekKey expectedSeekKey = new(matchSeeker.UserId, _casualPoolKey);
        await _openSeekNotifierMock
            .Received(1)
            .NotifyOpenSeekEndedAsync(
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(expectedUserIds)),
                matchSeeker.UserId,
                _casualPoolKey
            );
    }
}
