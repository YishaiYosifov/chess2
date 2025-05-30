using Akka.Actor;
using Akka.Hosting;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Player.Actors;
using Chess2.Api.Player.Models;
using FluentAssertions;
using NSubstitute;
using Xunit.Abstractions;

namespace Chess2.Api.Unit.Tests.PlayerTests;

public class PlayerActorTests : TestKit
{
    private readonly TestProbe _ratedPoolProbe;
    private readonly TestProbe _casualPoolProbe;
    private const string UserId = "test-user-id";

    private readonly IActorRef _playerActor;

    public PlayerActorTests(ITestOutputHelper output)
        : base(output: output)
    {
        _ratedPoolProbe = CreateTestProbe();
        _casualPoolProbe = CreateTestProbe();

        var ratedRequired = Substitute.For<IRequiredActor<RatedMatchmakingActor>>();
        ratedRequired.ActorRef.Returns(_ratedPoolProbe.Ref);

        var casualRequired = Substitute.For<IRequiredActor<CasualMatchmakingActor>>();
        casualRequired.ActorRef.Returns(_casualPoolProbe.Ref);

        _playerActor = Sys.ActorOf(
            Props.Create(() => new PlayerActor(UserId, ratedRequired, casualRequired))
        );
    }

    [Fact]
    public void CreateSeek_sends_command_to_RatedPool()
    {
        var poolInfo = new PoolInfo(BaseMinutes: 5, Increment: 5);
        var seek = new RatedMatchmakingCommands.CreateRatedSeek(UserId, 1700, poolInfo);

        _playerActor.Tell(new PlayerCommands.CreateSeek(UserId, seek));

        _ratedPoolProbe.ExpectMsg<RatedMatchmakingCommands.CreateRatedSeek>(msg =>
        {
            msg.UserId.Should().Be(UserId);
            msg.PoolInfo.Should().Be(poolInfo);
        });
    }

    [Fact]
    public void CreateSeek_sends_command_to_CasualPool()
    {
        var poolInfo = new PoolInfo(BaseMinutes: 5, Increment: 5);
        var seek = new CasualMatchmakingCommands.CreateCasualSeek(UserId, poolInfo);

        _playerActor.Tell(new PlayerCommands.CreateSeek(UserId, seek));

        _ratedPoolProbe.ExpectMsg<CasualMatchmakingCommands.CreateCasualSeek>(msg =>
        {
            msg.UserId.Should().Be(UserId);
            msg.PoolInfo.Should().Be(poolInfo);
        });
    }

    [Fact]
    public void CancelSeek_send_cancel_to_CurrentPool()
    {
        var poolInfo = new PoolInfo(BaseMinutes: 5, Increment: 5);
        var seek = new CasualMatchmakingCommands.CreateCasualSeek(UserId, poolInfo);

        _playerActor.Tell(new PlayerCommands.CreateSeek(UserId, seek));
        _casualPoolProbe.ExpectMsg<CasualMatchmakingCommands.CreateCasualSeek>();

        _playerActor.Tell(new PlayerCommands.CancelSeek(UserId));

        _casualPoolProbe.ExpectMsg<MatchmakingCommands.CancelSeek>(msg =>
        {
            msg.UserId.Should().Be(UserId);
            msg.PoolInfo.Should().Be(poolInfo);
        });
    }

    [Fact]
    public void CreateSeek_with_an_existing_seek_cancels_the_previous_one_first()
    {
        var casualPool = new PoolInfo(BaseMinutes: 5, Increment: 5);
        var ratedPool = new PoolInfo(BaseMinutes: 10, Increment: 0);

        var firstSeek = new CasualMatchmakingCommands.CreateCasualSeek(UserId, casualPool);
        var secondSeek = new RatedMatchmakingCommands.CreateRatedSeek(UserId, 1200, ratedPool);

        _playerActor.Tell(new PlayerCommands.CreateSeek(UserId, firstSeek));
        _casualPoolProbe.ExpectMsg<CasualMatchmakingCommands.CreateCasualSeek>();

        _playerActor.Tell(new PlayerCommands.CreateSeek(UserId, secondSeek));
        _casualPoolProbe.ExpectMsg<MatchmakingCommands.CancelSeek>(msg =>
        {
            msg.UserId.Should().Be(UserId);
            msg.PoolInfo.Should().Be(casualPool);
        });

        _ratedPoolProbe.ExpectMsg<RatedMatchmakingCommands.CreateRatedSeek>(msg =>
        {
            msg.UserId.Should().Be(UserId);
            msg.PoolInfo.Should().Be(ratedPool);
        });
    }
}
