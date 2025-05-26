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

public abstract class BaseMatchmakingActorTests<TPool> : TestKit where TPool : class, IMatchmakingPool
{
    protected readonly ITimerScheduler _timerMock = Substitute.For<ITimerScheduler>();
    protected readonly TPool _poolMock = Substitute.For<TPool>();

    protected readonly IActorRef _matchmakingActor;
    protected readonly TestProbe _probe;

    protected readonly PoolInfo _poolInfo = new(10, 5);

    protected abstract Props Props { get; }

    public BaseMatchmakingActorTests(ITestOutputHelper output)
        : base(output: output)
    {
        _matchmakingActor = Sys.ActorOf(Props);
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
            new MatchmakingCommands.CreateRatedSeek(userId, rating, _poolInfo),
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
            new MatchmakingCommands.CreateRatedSeek(userIdToKeep, 1300, _poolInfo),
            _probe.Ref
        );
        _matchmakingActor.Tell(
            new MatchmakingCommands.CreateRatedSeek(userIdToRemove, 1300, _poolInfo),
            _probe.Ref
        );
        _matchmakingActor.Tell(
            new MatchmakingCommands.CancelSeek(userIdToRemove, _poolInfo),
            _probe.Ref
        );

        Within(TimeSpan.FromSeconds(3), () => _poolMock.Received(1).RemoveSeek(userIdToRemove));
    }

}
