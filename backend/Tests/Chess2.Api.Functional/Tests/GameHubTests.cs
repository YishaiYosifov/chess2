using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Functional.Tests;

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
        var startGame = await GameUtils.CreateRatedGameAsync(DbContext, _gameService);

        await AssertMessage(
            startGame.GameToken,
            "test message from player",
            startGame.User1,
            startGame.User2
        );
    }

    [Fact]
    public async Task SendChatAsync_from_a_spectator_sends_to_spectators()
    {
        var startGame = await GameUtils.CreateRatedGameAsync(DbContext, _gameService);

        var spectator1 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        var spectator2 = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        await AssertMessage(
            startGame.GameToken,
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

        await Task.WhenAll(
            AssertMessage(game.GameToken, "player message", player, otherPlayer),
            AssertMessage(game.GameToken, "spectator message", spectator, otherSpectator)
        );
    }

    private async Task AssertMessage(
        string gameToken,
        string message,
        AuthedUser sender,
        params List<AuthedUser> receivers
    )
    {
        var allReceivers = receivers.Append(sender).ToList();

        List<TaskCompletionSource<(string sender, string message)>> tcsList = [];
        List<HubConnection> connections = [];
        foreach (var receiver in allReceivers)
        {
            TaskCompletionSource<(string sender, string message)> tcs = new();
            var conn = await ConnectSignalRAuthedAsync(GameHubPath(gameToken), receiver);
            conn.On<string, string>(
                "ChatMessageAsync",
                (sender, message) => tcs.TrySetResult((sender, message))
            );

            tcsList.Add(tcs);
            connections.Add(conn);
        }

        await connections[^1].InvokeAsync(SendChatMethod, gameToken, message, CT);

        foreach (var tcs in tcsList)
        {
            var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), CT);
            result.Should().BeEquivalentTo((sender.UserName, message));
        }

        foreach (var conn in connections)
            await conn.DisposeAsync();
    }
}
