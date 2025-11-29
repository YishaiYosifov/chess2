using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.SignalRClients;
using AnarchyChess.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Functional.Tests.LiveGameTests;

public class GameHubTests : BaseFunctionalTest
{
    private readonly IGameStarter _gameStarter;

    public GameHubTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _gameStarter = Scope.ServiceProvider.GetRequiredService<IGameStarter>();
    }

    [Fact]
    public async Task SyncRevisionAsync_fires_with_the_correct_revision_on_connection()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);

        await using GameHubClient player1Conn = new(
            AuthedSignalR(GameHubClient.Path(game.GameToken), game.User1),
            game.GameToken
        );
        await using GameHubClient player2ConnFirst = new(
            AuthedSignalR(GameHubClient.Path(game.GameToken), game.User2),
            game.GameToken
        );
        await player1Conn.StartAsync(CT);
        await player2ConnFirst.StartAsync(CT);

        (await player2ConnFirst.GetNextRevisionAsync(CT)).Should().Be(0);
        await player2ConnFirst.DisposeAsync();

        await player1Conn.RequestDrawAsync(CT);

        await using GameHubClient player2ConnReconnect = new(
            AuthedSignalR(GameHubClient.Path(game.GameToken), game.User2),
            game.GameToken
        );
        await player2ConnReconnect.StartAsync(CT);
        (await player2ConnReconnect.GetNextRevisionAsync(CT)).Should().Be(1);
    }
}
