using Chess2.Api.Matchmaking.Services.Pools;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public abstract class BaseMatchmakingActorTests<TPool> : BaseActorTest
    where TPool : class, IMatchmakingPool
{
    //protected ITimerScheduler TimerMock { get; } = Substitute.For<ITimerScheduler>();
    //protected TPool PoolMock { get; } = Substitute.For<TPool>();
    //protected ILiveGameService GameServiceMock { get; } = Substitute.For<ILiveGameService>();
    //protected IServiceProvider ServiceProviderMock { get; } = Substitute.For<IServiceProvider>();

    //protected IActorRef MatchmakingActor { get; }
    //protected TestProbe Probe { get; }

    //protected TimeControlSettings TimeControl { get; } = new(600, 5);
    //protected AppSettings Settings { get; } = AppSettingsLoader.LoadAppSettings();

    //protected abstract IActorRef CreateActor();

    //public BaseMatchmakingActorTests()
    //{
    //    MatchmakingActor = CreateActor();
    //    Probe = CreateTestProbe();
    //}

    //protected abstract ICreateSeekCommand CreateSeekCommand(string userId);
    //protected abstract void VerifySeekWasAdded(string userId, int times);

    //[Fact]
    //public void StartPeriodicTimer_is_set_for_match_waves()
    //{
    //    AwaitAssert(
    //        () =>
    //            TimerMock
    //                .Received(1)
    //                .StartPeriodicTimer(
    //                    "wave",
    //                    new MatchmakingCommands.MatchWave(),
    //                    Settings.Game.MatchWaveEvery
    //                ),
    //        cancellationToken: CT
    //    );
    //}

    //[Fact]
    //public async Task CancelSeek_removes_the_correct_user()
    //{
    //    const string userIdToRemove = "userToRemove";
    //    const string userIdToKeep = "userToKeep";

    //    PoolMock.RemoveSeek(userIdToRemove).Returns(true);
    //    PoolMock.RemoveSeek(userIdToKeep).Returns(true);

    //    MatchmakingActor.Tell(CreateSeekCommand(userIdToKeep), Probe);
    //    MatchmakingActor.Tell(CreateSeekCommand(userIdToRemove), Probe);
    //    MatchmakingActor.Tell(
    //        new MatchmakingCommands.CancelSeek(userIdToRemove, new(PoolType.Casual, TimeControl)),
    //        Probe
    //    );

    //    var cancelEvent = await Probe.FishForMessageAsync<MatchmakingReplies.SeekCanceled>(
    //        x => x.UserId == userIdToRemove,
    //        cancellationToken: CT
    //    );
    //    PoolMock.Received().RemoveSeek(userIdToRemove);
    //}

    //[Fact]
    //public async Task CreateSeek_watches_the_sender_and_triggers_cancel_on_termination()
    //{
    //    const string userId = "user1";

    //    MatchmakingActor.Tell(CreateSeekCommand(userId), Probe);
    //    await Probe.ExpectMsgAsync<MatchmakingReplies.SeekCreated>(cancellationToken: CT);
    //    Sys.Stop(Probe);

    //    await AwaitAssertAsync(() => PoolMock.Received().RemoveSeek(userId), cancellationToken: CT);
    //}

    //[Fact]
    //public async Task CreateSeek_doesnt_readd_the_seeker_if_it_already_exists()
    //{
    //    const string userId = "user1";

    //    MatchmakingActor.Tell(CreateSeekCommand(userId), Probe);
    //    await Probe.ExpectMsgAsync<MatchmakingReplies.SeekCreated>(cancellationToken: CT);

    //    VerifySeekWasAdded(userId, times: 1);
    //    PoolMock.HasSeek(userId).Returns(true);

    //    MatchmakingActor.Tell(CreateSeekCommand(userId), Probe);
    //    await Probe.ExpectMsgAsync<MatchmakingReplies.SeekCreated>(cancellationToken: CT);

    //    VerifySeekWasAdded(userId, times: 1);
    //}
}
