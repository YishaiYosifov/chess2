using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Functional.Tests;

using ChatTcs = TaskCompletionSource<(string sender, string message)>;

public class GameHubTests : BaseFunctionalTest
{
    private readonly ILiveGameService _gameService;
    private const string SendChatMethod = "SendChatAsync";

    public GameHubTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameService = Scope.ServiceProvider.GetRequiredService<ILiveGameService>();
    }

    private static string GameHubPath(string gameToken) => $"/api/hub/game?gameToken={gameToken}";

    [Fact]
    public async Task SendChatAsync_from_a_player_sends_to_players()
    {
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameService);

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
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameService);

        var spectator1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var spectator2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

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
        var game = await GameUtils.CreateRatedGameAsync(DbContext, _gameService);

        var player = game.User1;
        var otherPlayer = game.User2;

        var spectator = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var otherSpectator = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        string playerMessage = "player message";
        string spectatorMessage = "spectator message";

        var (playerTcs, playerConnections) = await ConnectToChatAsync(
            game.GameToken,
            player,
            otherPlayer
        );
        var playerSenderConn = playerConnections[0];

        var (spectatorTcs, spectatorConnections) = await ConnectToChatAsync(
            game.GameToken,
            spectator,
            otherSpectator
        );
        var spectatorSenderConn = spectatorConnections[0];

        await playerSenderConn.InvokeAsync(SendChatMethod, game.GameToken, playerMessage, CT);
        await spectatorSenderConn.InvokeAsync(SendChatMethod, game.GameToken, spectatorMessage, CT);

        await AssertAllReceiversGotMessageAsync(
            player,
            playerMessage,
            playerTcs,
            playerConnections
        );
        await AssertAllReceiversGotMessageAsync(
            spectator,
            spectatorMessage,
            spectatorTcs,
            spectatorConnections
        );
    }

    private async Task<(List<ChatTcs> tcsList, List<HubConnection> connections)> ConnectToChatAsync(
        string gameToken,
        params List<AuthedUser> receivers
    )
    {
        List<ChatTcs> tcsList = [];
        List<HubConnection> connections = [];
        foreach (var receiver in receivers)
        {
            ChatTcs tcs = new();
            var conn = await ConnectSignalRAuthedAsync(GameHubPath(gameToken), receiver);
            conn.On<string, string>(
                "ChatMessageAsync",
                (sender, message) => tcs.TrySetResult((sender, message))
            );

            tcsList.Add(tcs);
            connections.Add(conn);
        }

        return (tcsList, connections);
    }

    private async Task SendMessageAndAssertReceptionAsync(
        string gameToken,
        string message,
        AuthedUser sender,
        params List<AuthedUser> receivers
    )
    {
        var allReceivers = receivers.Append(sender).ToList();
        var (tcsList, connections) = await ConnectToChatAsync(gameToken, allReceivers);

        await connections[^1].InvokeAsync(SendChatMethod, gameToken, message, CT);

        await AssertAllReceiversGotMessageAsync(sender, message, tcsList, connections);
    }

    private async Task AssertAllReceiversGotMessageAsync(
        AuthedUser sender,
        string message,
        List<ChatTcs> tcsList,
        List<HubConnection> connections
    )
    {
        foreach (var tcs in tcsList)
        {
            var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), CT);
            result.Should().BeEquivalentTo((sender.UserName, message));
        }

        foreach (var conn in connections)
            await conn.DisposeAsync();
    }
}
