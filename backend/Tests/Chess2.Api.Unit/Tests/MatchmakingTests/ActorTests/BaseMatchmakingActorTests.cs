using Akka.Actor;
using Akka.TestKit;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Utils;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public abstract class BaseMatchmakingActorTests<TPool> : BaseActorTest
    where TPool : class, IMatchmakingPool
{
    protected ITimerScheduler TimerMock { get; } = Substitute.For<ITimerScheduler>();
    protected TPool PoolMock { get; } = Substitute.For<TPool>();
    protected ILiveGameService GameServiceMock { get; } = Substitute.For<ILiveGameService>();
    protected IServiceProvider ServiceProviderMock { get; } = Substitute.For<IServiceProvider>();

    protected IActorRef MatchmakingActor { get; }
    protected TestProbe Probe { get; }

    protected TimeControlSettings TimeControl { get; } = new(600, 5);
    protected AppSettings Settings { get; } = AppSettingsLoader.LoadAppSettings();

    protected abstract IActorRef CreateActor();

    public BaseMatchmakingActorTests()
    {
        var scopeMock = Substitute.For<IServiceScope>();
        scopeMock.ServiceProvider.GetService(typeof(ILiveGameService)).Returns(GameServiceMock);

        var scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        scopeFactoryMock.CreateScope().Returns(scopeMock);
        scopeMock
            .ServiceProvider.GetService(typeof(IServiceScopeFactory))
            .Returns(scopeFactoryMock);

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
                    ),
            cancellationToken: CT
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
        MatchmakingActor.Tell(
            new MatchmakingCommands.CancelSeek(userIdToRemove, TimeControl),
            Probe
        );

        var cancelEvent = await Probe.FishForMessageAsync<MatchmakingBroadcasts.SeekCanceled>(
            x => x.UserId == userIdToRemove,
            cancellationToken: CT
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
        await Probe.ExpectMsgAsync<MatchmakingBroadcasts.SeekCreated>(cancellationToken: CT);
        Sys.Stop(Probe);

        await listenerProbe.ExpectMsgAsync<MatchmakingBroadcasts.SeekCanceled>(
            x => x.UserId == userId,
            cancellationToken: CT
        );
        PoolMock.Received().RemoveSeek(userId);
    }
}
