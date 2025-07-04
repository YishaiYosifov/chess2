using Akka.Actor;
using Akka.Hosting;
using Akka.TestKit;
using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.SignalR;
using Chess2.Api.PlayerSession.Actors;
using Chess2.Api.PlayerSession.Models;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.PlayerTests;

public class PlayerSessionActorTests : BaseActorTest
{
    private readonly TestProbe _ratedPoolProbe;
    private readonly TestProbe _casualPoolProbe;
    private const string UserId = "test-user-id";

    private readonly IActorRef _playerSessionActor;
    private readonly IHubContext<MatchmakingHub, IMatchmakingClient> _matchmakingHubContextMock;

    public PlayerSessionActorTests()
    {
        _ratedPoolProbe = CreateTestProbe();
        _casualPoolProbe = CreateTestProbe();

        var ratedRequired = Substitute.For<IRequiredActor<RatedMatchmakingActor>>();
        ratedRequired.ActorRef.Returns(_ratedPoolProbe.Ref);

        var casualRequired = Substitute.For<IRequiredActor<CasualMatchmakingActor>>();
        casualRequired.ActorRef.Returns(_casualPoolProbe.Ref);

        _matchmakingHubContextMock = Substitute.For<
            IHubContext<MatchmakingHub, IMatchmakingClient>
        >();

        _playerSessionActor = Sys.ActorOf(
            Props.Create(
                () =>
                    new PlayerSessionActor(
                        UserId,
                        ratedRequired,
                        casualRequired,
                        _matchmakingHubContextMock
                    )
            )
        );
    }

    [Fact]
    public async Task CreateSeek_sends_command_to_RatedPool()
    {
        var timeControl = new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 5);
        var seek = new RatedMatchmakingCommands.CreateRatedSeek(UserId, 1700, timeControl);

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek(UserId, "connid", seek));

        await _ratedPoolProbe.ExpectMsgAsync<RatedMatchmakingCommands.CreateRatedSeek>(
            msg =>
            {
                msg.UserId.Should().Be(UserId);
                msg.TimeControl.Should().Be(timeControl);
            },
            cancellationToken: CT
        );
    }

    [Fact]
    public async Task CreateSeek_sends_command_to_CasualPool()
    {
        var timeControl = new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 5);
        var seek = new CasualMatchmakingCommands.CreateCasualSeek(UserId, timeControl);

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek(UserId, "connid", seek));

        await _casualPoolProbe.ExpectMsgAsync<CasualMatchmakingCommands.CreateCasualSeek>(
            msg =>
            {
                msg.UserId.Should().Be(UserId);
                msg.TimeControl.Should().Be(timeControl);
            },
            cancellationToken: CT
        );
    }

    [Fact]
    public async Task CancelSeek_send_cancel_to_CurrentPool()
    {
        var timeControl = new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 5);
        var seek = new CasualMatchmakingCommands.CreateCasualSeek(UserId, timeControl);

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek(UserId, "connid", seek));
        await _casualPoolProbe.ExpectMsgAsync<CasualMatchmakingCommands.CreateCasualSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(new PlayerSessionCommands.CancelSeek(UserId, "connid"));

        await _casualPoolProbe.ExpectMsgAsync<MatchmakingCommands.CancelSeek>(
            msg =>
            {
                msg.UserId.Should().Be(UserId);
                msg.TimeControl.Should().Be(timeControl);
            },
            cancellationToken: CT
        );
    }

    [Fact]
    public async Task CreateSeek_with_an_existing_seek_cancels_the_previous_one_first()
    {
        var casualTimeControl = new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 5);
        var ratedTimeControl = new TimeControlSettings(BaseSeconds: 600, IncrementSeconds: 0);

        var firstSeek = new CasualMatchmakingCommands.CreateCasualSeek(UserId, casualTimeControl);
        var secondSeek = new RatedMatchmakingCommands.CreateRatedSeek(
            UserId,
            1200,
            ratedTimeControl
        );

        _playerSessionActor.Tell(
            new PlayerSessionCommands.CreateSeek(UserId, "connid1", firstSeek)
        );
        await _casualPoolProbe.ExpectMsgAsync<CasualMatchmakingCommands.CreateCasualSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(
            new PlayerSessionCommands.CreateSeek(UserId, "connid2", secondSeek)
        );
        await _casualPoolProbe.ExpectMsgAsync<MatchmakingCommands.CancelSeek>(
            msg =>
            {
                msg.UserId.Should().Be(UserId);
                msg.TimeControl.Should().Be(casualTimeControl);
            },
            cancellationToken: CT
        );

        await _ratedPoolProbe.ExpectMsgAsync<RatedMatchmakingCommands.CreateRatedSeek>(
            msg =>
            {
                msg.UserId.Should().Be(UserId);
                msg.TimeControl.Should().Be(ratedTimeControl);
            },
            cancellationToken: CT
        );
    }

    [Fact]
    public async Task CancelSeek_without_a_connection_id_cancels_the_seek_anyways()
    {
        var timeControl = new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 5);
        var seek = new CasualMatchmakingCommands.CreateCasualSeek(UserId, timeControl);

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek(UserId, "connid", seek));
        await _casualPoolProbe.ExpectMsgAsync<CasualMatchmakingCommands.CreateCasualSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(new PlayerSessionCommands.CancelSeek(UserId));

        await _casualPoolProbe.ExpectMsgAsync<MatchmakingCommands.CancelSeek>(
            cancellationToken: CT
        );
    }
}
