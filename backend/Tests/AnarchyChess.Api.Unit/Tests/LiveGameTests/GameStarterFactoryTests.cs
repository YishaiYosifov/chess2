using AnarchyChess.Api.Game.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameStarterFactoryTests
{
    private readonly IGameStarter _gameStarterMock = Substitute.For<IGameStarter>();
    private readonly GameStarterFactory _gameStarterFactory;
    private readonly IServiceScope _scopeMock;

    public GameStarterFactoryTests()
    {
        var serviceProviderMock = Substitute.For<IServiceProvider>();
        serviceProviderMock.GetService(typeof(IGameStarter)).Returns(_gameStarterMock);

        _scopeMock = Substitute.For<IServiceScope, IAsyncDisposable>();
        _scopeMock.ServiceProvider.Returns(serviceProviderMock);

        var scopeFactoryMock = Substitute.For<IServiceScopeFactory>();
        scopeFactoryMock.CreateAsyncScope().Returns(_scopeMock);

        _gameStarterFactory = new(scopeFactoryMock);
    }

    [Fact]
    public async Task UseAsync_calls_action_with_game_starter_and_cancellation_token()
    {
        const string returnResult = "test";

        IGameStarter? capturedStarter = null;
        CancellationToken? capturedToken = null;
        var result = await _gameStarterFactory.UseAsync(
            (gameStarter, token) =>
            {
                capturedStarter = gameStarter;
                capturedToken = token;
                return Task.FromResult(returnResult);
            },
            TestContext.Current.CancellationToken
        );

        result.Should().Be(returnResult);
        capturedStarter.Should().Be(_gameStarterMock);
        capturedToken.Should().Be(TestContext.Current.CancellationToken);

        await ((IAsyncDisposable)_scopeMock).Received(1).DisposeAsync();
    }
}
