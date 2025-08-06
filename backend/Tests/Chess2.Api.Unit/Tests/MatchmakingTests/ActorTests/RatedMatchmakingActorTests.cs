using Akka.Actor;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public class RatedMatchmakingActorTests : BaseMatchmakingActorTests<IRatedMatchmakingPool>
{
    [Fact]
    public async Task CreateSeek_adds_the_user_to_seekers()
    {
        const string userId = "user1";
        const int rating = 1200;

        MatchmakingActor.Tell(
            new RatedMatchmakingCommands.CreateRatedSeek(userId, rating, TimeControl),
            Probe
        );
        await Probe.ExpectMsgAsync<MatchmakingReplies.SeekCreated>(
            x => x.UserId == userId,
            cancellationToken: CT
        );

        PoolMock.Received(1).AddSeek(userId, rating);
    }

    protected override ICreateSeekCommand CreateSeekCommand(string userId) =>
        new RatedMatchmakingCommands.CreateRatedSeek(userId, 1200, TimeControl);

    protected override void VerifySeekWasAdded(string userId, int times) =>
        PoolMock.Received(times).AddSeek(userId, Arg.Any<int>());

    protected override IActorRef CreateActor()
    {
        var props = Props.Create(
            () =>
                new RatedMatchmakingActor(
                    new PoolKey(PoolType.Rated, TimeControl).ToString(),
                    ServiceProviderMock,
                    Options.Create(Settings),
                    PoolMock,
                    TimerMock
                )
        );
        return Sys.ActorOf(props);
    }
}
