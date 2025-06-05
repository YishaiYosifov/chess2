using System.Collections.Immutable;
using Akka.Cluster.Sharding;
using Akka.Hosting;
using Akka.TestKit;
using Chess2.Api.Game.Actors;
using Chess2.Api.Game.Services;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.GameTests;

public class GameTokenGeneratorTests : BaseUnitTest
{
    private readonly GameTokenGenerator _tokenGenerator;

    private readonly TestProbe _gameActorProbe;

    public GameTokenGeneratorTests()
    {
        _gameActorProbe = CreateTestProbe();
        var requiredActorMock = Substitute.For<IRequiredActor<GameActor>>();
        requiredActorMock.ActorRef.Returns(_gameActorProbe);

        _tokenGenerator = new(requiredActorMock);
    }

    [Fact]
    public async Task GenerateUniqueGameToken_ReturnsUniqueToken_NotInExistingActors()
    {
        var existingTokens = new HashSet<string> { "existingtoken1", "existingtoken2" };

        var currentShardRegionState = new CurrentShardRegionState(
            existingTokens.Select(t => new ShardState("shardId", [t])).ToImmutableHashSet(),
            []
        );

        var act = Task.Run(_tokenGenerator.GenerateUniqueGameToken);

        await _gameActorProbe.ExpectMsgAsync<GetShardRegionState>(cancellationToken: CT);
        _gameActorProbe.Reply(currentShardRegionState);

        var token = await act;

        token.Should().NotBeNull();
        token.Should().HaveLength(16);
        existingTokens.Should().NotContain(token);
    }
}
