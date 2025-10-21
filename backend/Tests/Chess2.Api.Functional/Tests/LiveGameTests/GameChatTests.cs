using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.Profile.Entities;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.SignalRClients;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Functional.Tests.LiveGameTests;

public class GameChatTests : BaseFunctionalTest
{
    private readonly IGameStarter _gameStarter;

    public GameChatTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameStarter = Scope.ServiceProvider.GetRequiredService<IGameStarter>();
    }

    [Fact]
    public async Task SendChatAsync_from_a_player_sends_to_players()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);

        await SendMessageAndAssertReceptionAsync(
            game.GameToken,
            "test message from player",
            game.User1,
            game.User2
        );
    }

    [Fact]
    public async Task SendChatAsync_from_a_spectator_sends_to_spectators()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);
        var spectator1 = new AuthedUserFaker().Generate();
        var spectator2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(spectator1, spectator2);
        await DbContext.SaveChangesAsync(CT);

        await SendMessageAndAssertReceptionAsync(
            game.GameToken,
            "test message from spectator",
            spectator1,
            spectator2
        );
    }

    [Fact]
    public async Task SendChatAsync_player_and_spectator_send_simultaneous_messages_to_their_own_groups()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameStarter);
        var gameToken = game.GameToken;
        var player1 = game.User1;
        var player2 = game.User2;

        var spectator1 = new AuthedUserFaker().Generate();
        var spectator2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(spectator1, spectator2);
        await DbContext.SaveChangesAsync(CT);

        string playerMessage = "player message";
        string spectatorMessage = "spectator message";

        await using var playerConn1 = await GameHubClient.ConnectToChatAsync(
            await AuthedSignalRAsync(GameHubClient.Path(gameToken), player1),
            gameToken,
            CT
        );
        await using var playerConn2 = await GameHubClient.ConnectToChatAsync(
            await AuthedSignalRAsync(GameHubClient.Path(gameToken), player2),
            gameToken,
            CT
        );

        await using var specConn1 = await GameHubClient.ConnectToChatAsync(
            await AuthedSignalRAsync(GameHubClient.Path(gameToken), spectator1),
            gameToken,
            CT
        );
        await using var specConn2 = await GameHubClient.ConnectToChatAsync(
            await AuthedSignalRAsync(GameHubClient.Path(gameToken), spectator2),
            gameToken,
            CT
        );

        await playerConn1.SendChatAsync(playerMessage, CT);
        await specConn1.SendChatAsync(spectatorMessage, CT);

        await AssertAllReceiversGotMessageAsync(player1, playerMessage, playerConn1, playerConn2);
        await AssertAllReceiversGotMessageAsync(spectator1, spectatorMessage, specConn1, specConn2);
    }

    private async Task SendMessageAndAssertReceptionAsync(
        GameToken gameToken,
        string message,
        AuthedUser sender,
        params List<AuthedUser> receivers
    )
    {
        var allReceivers = receivers.Append(sender).ToList();
        var connections = await Task.WhenAll(
            allReceivers.Select(async x =>
                await GameHubClient.ConnectToChatAsync(
                    await AuthedSignalRAsync(GameHubClient.Path(gameToken), x),
                    gameToken,
                    CT
                )
            )
        );

        await connections[^1].SendChatAsync(message, CT);
        await AssertAllReceiversGotMessageAsync(sender, message, connections);

        foreach (var conn in connections)
            await conn.DisposeAsync();
    }

    private async Task AssertAllReceiversGotMessageAsync(
        AuthedUser sender,
        string message,
        params IEnumerable<GameHubClient> connections
    )
    {
        foreach (var conn in connections)
        {
            var result = await conn.GetNextMessageAsync(CT);
            result.Should().BeEquivalentTo((sender.UserName, message));
        }

        foreach (var conn in connections)
            await conn.DisposeAsync();
    }
}
