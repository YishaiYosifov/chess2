using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Chess2.Api.Game.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Utils;
using NSubstitute;
using Xunit.Abstractions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public abstract class BaseMatchmakingActorTests<TPool> : TestKit
    where TPool : class, IMatchmakingPool
{
    protected ITimerScheduler TimerMock { get; } = Substitute.For<ITimerScheduler>();
    protected TPool PoolMock { get; } = Substitute.For<TPool>();
    protected IGameService GameServiceMock { get; } = Substitute.For<IGameService>();

    protected IActorRef MatchmakingActor { get; }
    protected TestProbe Probe { get; }

    protected PoolInfo PoolInfo { get; } = new(10, 5);
    protected AppSettings Settings { get; } = AppSettingsLoader.LoadAppSettings();

    protected abstract IActorRef CreateActor();

    public BaseMatchmakingActorTests(ITestOutputHelper output)
        : base(output: output)
    {
        MatchmakingActor = CreateActor();
        Probe = CreateTestProbe();

        Sys.EventStream.Subscribe(Probe, typeof(MatchmakingBroadcasts.SeekCreated));
        Sys.EventStream.Subscribe(Probe, typeof(MatchmakingBroadcasts.SeekCanceled));
    }

    protected abstract ICreateSeekCommand CreateSeekCommand(string userId);

    [Fact]
    public void StartPeriodicTimer_is_set_for_match_waves()
    {
        AwaitAssert(
            () =>
                TimerMock
                    .Received(1)
                    .StartPeriodicTimer(
                        "wave",
                        new MatchmakingCommands.MatchWave(),
                        Settings.Game.MatchWaveEvery
                    )
        );
    }

    [Fact]
    public async Task CancelSeek_removes_the_correct_user()
    {
        const string userIdToRemove = "userToRemove";
        const string userIdToKeep = "userToKeep";

        PoolMock.RemoveSeek(userIdToRemove).Returns(true);
        PoolMock.RemoveSeek(userIdToKeep).Returns(true);

        MatchmakingActor.Tell(CreateSeekCommand(userIdToKeep), Probe);
        MatchmakingActor.Tell(CreateSeekCommand(userIdToRemove), Probe);
        MatchmakingActor.Tell(new MatchmakingCommands.CancelSeek(userIdToRemove, PoolInfo), Probe);

        var cancelEvent = await Probe.FishForMessageAsync<MatchmakingBroadcasts.SeekCanceled>(x =>
            x.UserId == userIdToRemove
        );
        PoolMock.Received().RemoveSeek(userIdToRemove);
    }

    [Fact]
    public async Task CreateSeek_watches_the_sender_and_triggers_cancel_on_termination()
    {
        const string userId = "user1";

        PoolMock.RemoveSeek(userId).Returns(true);

        var listenerProbe = CreateTestProbe("listener");
        Sys.EventStream.Subscribe(listenerProbe, typeof(MatchmakingBroadcasts.SeekCanceled));

        MatchmakingActor.Tell(CreateSeekCommand(userId), Probe);
        await Probe.ExpectMsgAsync<MatchmakingBroadcasts.SeekCreated>();
        Sys.Stop(Probe);

        await listenerProbe.ExpectMsgAsync<MatchmakingBroadcasts.SeekCanceled>(x =>
            x.UserId == userId
        );
        PoolMock.Received().RemoveSeek(userId);
    }
}
