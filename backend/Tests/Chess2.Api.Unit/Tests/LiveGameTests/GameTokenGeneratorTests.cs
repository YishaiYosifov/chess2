using Chess2.Api.LiveGame.Grains;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Services;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameTokenGeneratorTests
{
    private readonly GameTokenGenerator _tokenGenerator;

    private readonly IGrainFactory _grainFactoryMock = Substitute.For<IGrainFactory>();
    private readonly IRandomCodeGenerator _randomCodeGeneratorMock =
        Substitute.For<IRandomCodeGenerator>();

    public GameTokenGeneratorTests()
    {
        _tokenGenerator = new(_grainFactoryMock, _randomCodeGeneratorMock);
    }

    [Fact]
    public async Task GenerateUniqueGameToken_returns_a_unique_token()
    {
        GameToken existingToken = "existingToken";
        GameToken nonExistingToken = "nonExistingToken";
        _randomCodeGeneratorMock
            .GenerateBase62Code(Arg.Any<int>())
            .ReturnsForAnyArgs(existingToken.Value, nonExistingToken.Value);

        var existingTokenGrain = Substitute.For<IGameGrain>();
        var nonExistingTokenGrain = Substitute.For<IGameGrain>();

        existingTokenGrain.DoesGameExistAsync().Returns(true);
        nonExistingTokenGrain.DoesGameExistAsync().Returns(false);

        _grainFactoryMock.GetGrain<IGameGrain>(existingToken).Returns(existingTokenGrain);
        _grainFactoryMock.GetGrain<IGameGrain>(nonExistingToken).Returns(nonExistingTokenGrain);

        var token = await _tokenGenerator.GenerateUniqueGameToken();

        token.Should().Be(nonExistingToken);
    }
}
