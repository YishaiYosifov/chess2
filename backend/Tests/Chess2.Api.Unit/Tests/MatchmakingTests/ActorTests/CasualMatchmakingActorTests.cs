using Akka.Actor;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public class CasualMatchmakingActorTests(ITestOutputHelper output)
    : BaseMatchmakingActorTests<ICasualMatchmakingPool>(output: output)
{
    [Fact]
    public async Task CreateSeek_adds_the_user_to_seekers()
    {
        const string userId = "user1";

        MatchmakingActor.Tell(CreateSeekCommand(userId), Probe);
        await Probe.ExpectMsgAsync<MatchmakingBroadcasts.SeekCreated>(x => x.UserId == userId);

        PoolMock.Received(1).AddSeek(userId);
    }

    protected override ICreateSeekCommand CreateSeekCommand(string userId) =>
            new CasualMatchmakingCommands.CreateCasualSeek(userId, PoolInfo);

    protected override IActorRef CreateActor()
    {
        var props = Props.Create(
            () => new CasualMatchmakingActor(Options.Create(Settings), PoolMock, TimerMock)
        );
        return Sys.ActorOf(props);
    }
}
