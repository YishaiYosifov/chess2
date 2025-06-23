using Akka.Actor;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public class CasualMatchmakingActorTests : BaseMatchmakingActorTests<ICasualMatchmakingPool>
{
    [Fact]
    public async Task CreateSeek_adds_the_user_to_seekers()
    {
        const string userId = "user1";

        MatchmakingActor.Tell(CreateSeekCommand(userId), Probe);
        await Probe.ExpectMsgAsync<MatchmakingBroadcasts.SeekCreated>(
            x => x.UserId == userId,
            cancellationToken: CT
        );

        PoolMock.Received(1).AddSeek(userId);
    }

    protected override ICreateSeekCommand CreateSeekCommand(string userId) =>
        new CasualMatchmakingCommands.CreateCasualSeek(userId, TimeControl);

    protected override IActorRef CreateActor()
    {
        var props = Props.Create(
            () =>
                new CasualMatchmakingActor(
                    Options.Create(Settings),
                    PoolMock,
                    GameServiceMock,
                    TimerMock
                )
        );
        return Sys.ActorOf(props);
    }
}
