﻿using Chess2.Api.Game.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.SignalRClients;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Functional.Tests.LiveGameTests;

public class GameHubTests : BaseFunctionalTest
{
    private readonly IGameStarter _gameStarter;

    public GameHubTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameStarter = Scope.ServiceProvider.GetRequiredService<IGameStarter>();
    }

    [Fact]
    public async Task SyncRevisionAsync_fires_with_the_correct_revision_on_connection()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);

        await using GameHubClient player1Conn = new(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User1),
            game.GameToken
        );
        await using GameHubClient player2ConnFirst = new(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User2),
            game.GameToken
        );

        (await player2ConnFirst.GetNextRevisionAsync(CT)).Should().Be(0);
        await player2ConnFirst.DisposeAsync();

        await player1Conn.RequestDrawAsync(CT);

        await using GameHubClient player2ConnReconnect = new(
            await AuthedSignalRAsync(GameHubClient.Path(game.GameToken), game.User2),
            game.GameToken
        );
        (await player2ConnReconnect.GetNextRevisionAsync(CT)).Should().Be(1);
    }
}
