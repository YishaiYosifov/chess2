using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
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
    }

    protected abstract void AddSeekToPool(string userId);

    [Fact]
    public void StartPeriodicTimer_is_set_for_match_waves()
    {
        TimerMock
            .Received(1)
            .StartPeriodicTimer(
                "wave",
                new MatchmakingCommands.MatchWave(),
                Settings.Game.MatchWaveEvery
            );
    }

    [Fact]
    public void CancelSeek_removes_the_correct_user()
    {
        const string userIdToRemove = "userToRemove";
        const string userIdToKeep = "userToKeep";

        AddSeekToPool(userIdToKeep);
        AddSeekToPool(userIdToRemove);
        MatchmakingActor.Tell(
            new MatchmakingCommands.CancelSeek(userIdToRemove, PoolInfo),
            Probe.Ref
        );

        AwaitAssert(() => PoolMock.Received().RemoveSeek(userIdToRemove));
    }
}
