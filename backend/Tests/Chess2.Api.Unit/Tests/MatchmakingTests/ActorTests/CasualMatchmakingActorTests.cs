using Chess2.Api.Matchmaking.Services.Pools;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public class CasualMatchmakingActorTests : BaseMatchmakingActorTests<ICasualMatchmakingPool>
{
    //[Fact]
    //public async Task CreateSeek_adds_the_user_to_seekers()
    //{
    //    const string userId = "user1";

    //    MatchmakingActor.Tell(CreateSeekCommand(userId), Probe);
    //    await Probe.ExpectMsgAsync<MatchmakingReplies.SeekCreated>(
    //        x => x.UserId == userId,
    //        cancellationToken: CT
    //    );

    //    PoolMock.Received(1).AddSeek(userId);
    //}

    //protected override ICreateSeekCommand CreateSeekCommand(string userId) =>
    //    new CasualMatchmakingCommands.CreateCasualSeek(userId, TimeControl);

    //protected override void VerifySeekWasAdded(string userId, int times) =>
    //    PoolMock.Received(times).AddSeek(userId);

    //protected override IActorRef CreateActor()
    //{
    //    var props = Props.Create(
    //        () =>
    //            new CasualMatchmakingActor(
    //                new PoolKey(PoolType.Casual, TimeControl).ToString(),
    //                ServiceProviderMock,
    //                Options.Create(Settings),
    //                PoolMock,
    //                TimerMock
    //            )
    //    );
    //    return Sys.ActorOf(props);
    //}
}
