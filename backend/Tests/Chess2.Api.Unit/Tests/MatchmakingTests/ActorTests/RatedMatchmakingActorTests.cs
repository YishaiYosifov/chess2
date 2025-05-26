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

namespace Chess2.Api.Unit.Tests.MatchmakingTests.ActorTests;

public class RatedMatchmakingActorTests : BaseMatchmakingActorTests<IRatedMatchmakingPool>
{
    private readonly AppSettings _settings;

    protected override Props Props { get; }

    public RatedMatchmakingActorTests(ITestOutputHelper output)
        : base(output: output)
    {
        _settings = AppSettingsLoader.LoadAppSettings();

        Props = Props.Create(
            () => new RatedMatchmakingActor(Options.Create(_settings), _poolMock, _timerMock)
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
