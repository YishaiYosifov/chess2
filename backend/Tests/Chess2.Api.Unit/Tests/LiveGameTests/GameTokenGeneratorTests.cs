using Akka.Hosting;
using Akka.TestKit;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Services;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameTokenGeneratorTests : BaseActorTest
{
    private readonly GameTokenGenerator _tokenGenerator;
    private readonly TestProbe _gameActorProbe;

    private readonly IRandomCodeGenerator _randomCodeGeneratorMock =
        Substitute.For<IRandomCodeGenerator>();

    public GameTokenGeneratorTests()
    {
        _gameActorProbe = CreateTestProbe();
        var requiredActorMock = Substitute.For<IRequiredActor<GameGrain>>();
        requiredActorMock.ActorRef.Returns(_gameActorProbe);

        _tokenGenerator = new(requiredActorMock, _randomCodeGeneratorMock);
    }

    [Fact]
    public async Task GenerateUniqueGameToken_returns_a_unique_token()
    {
        var existingToken = "existinToken";
        var nonExistingToken = "nonExistingToken";
        _randomCodeGeneratorMock
            .GenerateBase62Code(Arg.Any<int>())
            .ReturnsForAnyArgs(existingToken, nonExistingToken);

        var act = Task.Run(_tokenGenerator.GenerateUniqueGameToken);

        // first token taken
        await _gameActorProbe.ExpectMsgAsync<GameQueries.IsGameOngoing>(cancellationToken: CT);
        _gameActorProbe.Reply(true);

        // second token doesn't exist
        await _gameActorProbe.ExpectMsgAsync<GameQueries.IsGameOngoing>(cancellationToken: CT);
        _gameActorProbe.Reply(false);

        var token = await act;

        token.Should().Be(nonExistingToken);
    }
}
