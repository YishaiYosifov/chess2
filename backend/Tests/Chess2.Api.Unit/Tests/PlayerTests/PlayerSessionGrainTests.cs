using Akka.Actor;
using Akka.Hosting;
using Akka.TestKit;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Grains;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.PlayerSession.Models;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.PlayerTests;

public class PlayerSessionGrainTests : BaseGrainTest
{
    private readonly TestProbe _ratedPoolProbe;
    private readonly TestProbe _casualPoolProbe;
    private readonly TestProbe _probe;
    private const string UserId = "test-user-id";

    private readonly IActorRef _playerSessionActor;
    private readonly IMatchmakingNotifier _matchmakingNotifierMock;

    public PlayerSessionGrainTests()
    {
        _ratedPoolProbe = CreateTestProbe();
        _casualPoolProbe = CreateTestProbe();
        _probe = CreateTestProbe();

        var ratedRequired = Substitute.For<IRequiredActor<RatedMatchmakingGrain>>();
        ratedRequired.ActorRef.Returns(_ratedPoolProbe.Ref);

        var casualRequired = Substitute.For<IRequiredActor<CasualMatchmakingGrain>>();
        casualRequired.ActorRef.Returns(_casualPoolProbe.Ref);

        _matchmakingNotifierMock = Substitute.For<IMatchmakingNotifier>();

        _playerSessionActor = Sys.ActorOf(
            Props.Create(
                () =>
                    new PlayerSessionActor(
                        UserId,
                        ratedRequired,
                        casualRequired,
                        _matchmakingNotifierMock
                    )
            )
        );
    }

    [Fact]
    public async Task CreateSeek_sends_seek_to_the_correct_pool()
    {
        var seekCommand = new RatedMatchmakingCommands.CreateRatedSeek(
            "user1",
            1500,
            new TimeControlSettings(300, 5)
        );
        var createSeek = new PlayerSessionCommands.CreateSeek("user1", "conn1", seekCommand);

        _playerSessionActor.Tell(createSeek);

        var received =
            await _ratedPoolProbe.ExpectMsgAsync<RatedMatchmakingCommands.CreateRatedSeek>(
                cancellationToken: CT
            );
        received.Should().BeEquivalentTo(seekCommand);
    }

    [Fact]
    public async Task CreateSeek_cancels_previous_seek_when_the_connection_id_is_reused()
    {
        var firstCreate = new CasualMatchmakingCommands.CreateCasualSeek(
            "user1",
            new TimeControlSettings(60, 0)
        );
        var secondCreate = new RatedMatchmakingCommands.CreateRatedSeek(
            "user1",
            1800,
            new TimeControlSettings(300, 5)
        );

        _playerSessionActor.Tell(
            new PlayerSessionCommands.CreateSeek("user1", "conn1", firstCreate)
        );
        await _casualPoolProbe.ExpectMsgAsync<CasualMatchmakingCommands.CreateCasualSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(
            new PlayerSessionCommands.CreateSeek("user1", "conn1", secondCreate)
        );

        var cancel = await _casualPoolProbe.ExpectMsgAsync<MatchmakingCommands.CancelSeek>(
            cancellationToken: CT
        );
        cancel.Key.Should().Be(firstCreate.Key);

        var forwardedSecondCreate =
            await _ratedPoolProbe.ExpectMsgAsync<RatedMatchmakingCommands.CreateRatedSeek>(
                cancellationToken: CT
            );
        forwardedSecondCreate.Should().BeEquivalentTo(secondCreate);
    }

    [Fact]
    public async Task MatchFound_notifies_all_connections_listening_to_the_same_pool()
    {
        var command = new RatedMatchmakingCommands.CreateRatedSeek(
            "user1",
            1200,
            new TimeControlSettings(180, 2)
        );
        var gameToken = "game 123";

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek("user1", "connA", command));
        await _ratedPoolProbe.ExpectMsgAsync<RatedMatchmakingCommands.CreateRatedSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek("user1", "connB", command));
        await _ratedPoolProbe.ExpectMsgAsync<RatedMatchmakingCommands.CreateRatedSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(new MatchmakingEvents.MatchFound(gameToken, command.Key));

        await AwaitAssertAsync(
            () =>
            {
                _matchmakingNotifierMock.Received(1).NotifyGameFoundAsync("connA", gameToken);
                _matchmakingNotifierMock.Received(1).NotifyGameFoundAsync("connB", gameToken);
            },
            cancellationToken: CT
        );
    }

    [Fact]
    public async Task CancelSeek_cleans_up_seek()
    {
        var command = new CasualMatchmakingCommands.CreateCasualSeek(
            "user1",
            new TimeControlSettings(10, 1)
        );

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek("user1", "conn1", command));
        await _casualPoolProbe.ExpectMsgAsync<CasualMatchmakingCommands.CreateCasualSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(new PlayerSessionCommands.CancelSeek("user1", "conn1"));
        await _casualPoolProbe.ExpectMsgAsync<MatchmakingCommands.CancelSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(new MatchmakingEvents.MatchFound("game789", command.Key), _probe);
        await _probe.ExpectMsgAsync<PlayerSessionReplies.MatchFound>(cancellationToken: CT);

        await _matchmakingNotifierMock
            .Received(0)
            .NotifyGameFoundAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CancelSeek_only_removes_the_correct_seek()
    {
        var command = new RatedMatchmakingCommands.CreateRatedSeek(
            "user1",
            2000,
            new TimeControlSettings(600, 10)
        );

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek("user1", "connX", command));
        await _ratedPoolProbe.ExpectMsgAsync<RatedMatchmakingCommands.CreateRatedSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(new PlayerSessionCommands.CreateSeek("user1", "connY", command));
        await _ratedPoolProbe.ExpectMsgAsync<RatedMatchmakingCommands.CreateRatedSeek>(
            cancellationToken: CT
        );

        _playerSessionActor.Tell(new PlayerSessionCommands.CancelSeek("user1", "connX"));
        var cancel = await _ratedPoolProbe.ExpectMsgAsync<MatchmakingCommands.CancelSeek>(
            cancellationToken: CT
        );
        cancel.Key.Should().Be(command.Key);

        _playerSessionActor.Tell(new MatchmakingEvents.MatchFound("gameY", command.Key), _probe);
        await _probe.ExpectMsgAsync<PlayerSessionReplies.MatchFound>(cancellationToken: CT);
        await _matchmakingNotifierMock.Received(1).NotifyGameFoundAsync("connY", "gameY");
    }
}
