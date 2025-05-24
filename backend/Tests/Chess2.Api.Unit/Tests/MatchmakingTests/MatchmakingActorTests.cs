using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Chess2.Api.Game.Models;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Utils;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit.Abstractions;

namespace Chess2.Api.Unit.Tests.MatchmakingTests;

public class MatchmakingActorTests : TestKit
{
    private readonly ITimerScheduler _timerMock = Substitute.For<ITimerScheduler>();
    private readonly IMatchmakingPool _poolMock = Substitute.For<IMatchmakingPool>();

    private readonly IActorRef _matchmakingActor;
    private readonly TestProbe _probe;

    private readonly AppSettings _settings;
    private readonly TimeControlInfo _timeControl = new(10, 5);

    public MatchmakingActorTests(ITestOutputHelper output)
        : base(output: output)
    {
        _settings = AppSettingsLoader.LoadAppSettings();

        var props = Props.Create(
            () => new MatchmakingActor(Options.Create(_settings), _poolMock, _timerMock)
        );
        _matchmakingActor = Sys.ActorOf(props);
        _probe = CreateTestProbe();
    }

    [Fact]
    public void StartPeriodicTimer_is_set_for_match_waves()
    {
        _timerMock
            .Received(1)
            .StartPeriodicTimer(
                "wave",
                new MatchmakingCommands.MatchWave(),
                _settings.Game.MatchWaveEvery
            );
    }

    [Fact]
    public void CreateSeek_adds_the_user_to_seekers()
    {
        const string userId = "user1";
        const int rating = 1200;

        _matchmakingActor.Tell(
            new MatchmakingCommands.CreateSeek(userId, rating, _timeControl),
            _probe.Ref
        );

        Within(TimeSpan.FromSeconds(3), () => _poolMock.Received(1).AddSeek(userId, rating));
    }

    [Fact]
    public void CancelSeek_removes_the_correct_user()
    {
        const string userIdToRemove = "userToRemove";
        const string userIdToKeep = "userToKeep";

        _matchmakingActor.Tell(
            new MatchmakingCommands.CreateSeek(userIdToKeep, 1300, _timeControl),
            _probe.Ref
        );
        _matchmakingActor.Tell(
            new MatchmakingCommands.CreateSeek(userIdToRemove, 1300, _timeControl),
            _probe.Ref
        );
        _matchmakingActor.Tell(
            new MatchmakingCommands.CancelSeek(userIdToRemove, _timeControl),
            _probe.Ref
        );

        Within(TimeSpan.FromSeconds(3), () => _poolMock.Received(1).RemoveSeek(userIdToRemove));
    }

    [Fact]
    public async Task MatchWave_notifies_all_matched_subscribers_and_removes_their_seek()
    {
        const string userId1 = "user1";
        const string userId2 = "user2";
        const string unmatchedUserId = "unmatched user";

        var probe1 = CreateTestProbe();
        var probe2 = CreateTestProbe();
        var unmatchedProbe = CreateTestProbe();

        probe1.Send(
            _matchmakingActor,
            new MatchmakingCommands.CreateSeek(userId1, 1500, _timeControl)
        );
        probe2.Send(
            _matchmakingActor,
            new MatchmakingCommands.CreateSeek(userId2, 1600, _timeControl)
        );
        unmatchedProbe.Send(
            _matchmakingActor,
            new MatchmakingCommands.CreateSeek(unmatchedUserId, 1600, _timeControl)
        );
        _poolMock.CalculateMatches().Returns([(userId1, userId2)]);

        _matchmakingActor.Tell(new MatchmakingCommands.MatchWave());

        _poolMock.Received().RemoveSeek(userId1);
        _poolMock.Received().RemoveSeek(userId2);
        _poolMock.DidNotReceive().RemoveSeek(unmatchedUserId);

        // Each subscriber should receive a MatchFound message with the opponent's userId
        await probe1.ExpectMsgAsync<MatchmakingEvents.MatchFound>(msg => msg.OpponentId == userId2);
        await probe2.ExpectMsgAsync<MatchmakingEvents.MatchFound>(msg => msg.OpponentId == userId1);
        await unmatchedProbe.ExpectNoMsgAsync();
    }

    [Fact]
    public async Task MatchWave_unsubscribers_sender_after_a_match()
    {
        const string userId1 = "user1";
        const string userId2 = "user2";
        var probe1 = CreateTestProbe();
        var probe2 = CreateTestProbe();
        probe1.Send(
            _matchmakingActor,
            new MatchmakingCommands.CreateSeek(userId1, 1600, _timeControl)
        );
        probe2.Send(
            _matchmakingActor,
            new MatchmakingCommands.CreateSeek(userId2, 1600, _timeControl)
        );

        _poolMock.CalculateMatches().Returns([(userId1, userId2)]);

        _matchmakingActor.Tell(new MatchmakingCommands.MatchWave());
        await probe1.ExpectMsgAsync<MatchmakingEvents.MatchFound>();
        await probe2.ExpectMsgAsync<MatchmakingEvents.MatchFound>();

        // the matchmaker still returns the same users, but they should now be unsubscribed
        _matchmakingActor.Tell(new MatchmakingCommands.MatchWave());
        await probe1.ExpectNoMsgAsync();
        await probe2.ExpectNoMsgAsync();
    }
}
