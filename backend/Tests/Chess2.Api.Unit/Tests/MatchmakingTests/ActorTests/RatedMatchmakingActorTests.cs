using Akka.Actor;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit.Abstractions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public class RatedMatchmakingActorTests(ITestOutputHelper output)
    : BaseMatchmakingActorTests<IRatedMatchmakingPool>(output: output)
{
    [Fact]
    public async Task CreateSeek_adds_the_user_to_seekers()
    {
        const string userId = "user1";
        const int rating = 1200;

        MatchmakingActor.Tell(
            new RatedMatchmakingCommands.CreateRatedSeek(userId, rating, PoolInfo),
            Probe
        );
        await Probe.ExpectMsgAsync<MatchmakingBroadcasts.SeekCreated>(x => x.UserId == userId);

        PoolMock.Received(1).AddSeek(userId, rating);
    }

    protected override ICreateSeekCommand CreateSeekCommand(string userId) =>
        new RatedMatchmakingCommands.CreateRatedSeek(userId, 1200, PoolInfo);

    protected override IActorRef CreateActor()
    {
        var props = Props.Create(
            () =>
                new RatedMatchmakingActor(
                    Options.Create(Settings),
                    PoolMock,
                    GameServiceMock,
                    TimerMock
                )
        );
        return Sys.ActorOf(props);
    }
}
