using System.Threading.Channels;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.TestInfrastructure.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class GameHubClient : BaseHubClient
{
    public static string Path(GameToken gameToken) => $"/api/hub/game?gameToken={gameToken}";

    private readonly Channel<(string SenderUserName, string Message)> _messagesChannel =
        Channel.CreateUnbounded<(string, string)>();

    private readonly Channel<bool> _rematchRequestedChannel = Channel.CreateUnbounded<bool>();
    private readonly Channel<bool> _rematchCancelledChannel = Channel.CreateUnbounded<bool>();
    private readonly Channel<GameToken> _rematchAcceptedChannel =
        Channel.CreateUnbounded<GameToken>();

    private readonly TaskCompletionSource _connectedTcs = new();
    private readonly string _gameToken;

    public GameHubClient(HubConnection connection, string gameToken)
        : base(connection)
    {
        _gameToken = gameToken;

        Connection.On("ChatConnectedAsync", _connectedTcs.SetResult);
        Connection.On<string, string>(
            "ChatMessageAsync",
            (senderUserName, message) => _messagesChannel.Writer.TryWrite((senderUserName, message))
        );

        Connection.On(
            "RematchRequestedAsync",
            () => _rematchRequestedChannel.Writer.TryWrite(true)
        );
        Connection.On(
            "RematchCancelledAsync",
            () => _rematchCancelledChannel.Writer.TryWrite(true)
        );
        Connection.On<GameToken>(
            "RematchAccepted",
            gameToken => _rematchAcceptedChannel.Writer.TryWrite(gameToken)
        );
    }

    public static async Task<GameHubClient> ConnectToChatAsync(
        HubConnection connection,
        GameToken gameToken,
        CancellationToken token
    )
    {
        GameHubClient client = new(connection, gameToken);
        await client.WaitForChatConnection(token);
        return client;
    }

    public Task WaitForChatConnection(CancellationToken token) =>
        _connectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10), token);

    public Task SendChatAsync(string message, CancellationToken token) =>
        Connection.InvokeAsync("SendChatAsync", _gameToken, message, token);

    public async Task<(string SenderUserName, string Message)> GetNextMessageAsync(
        CancellationToken token = default
    ) => await _messagesChannel.Reader.ReadAsync(token.WithTimeout(TimeSpan.FromSeconds(10)));

    public async Task WaitForRematchRequestedAsync(CancellationToken token = default) =>
        await _rematchRequestedChannel.Reader.ReadAsync(
            token.WithTimeout(TimeSpan.FromSeconds(10))
        );

    public async Task WaitForRematchCancelledAsync(CancellationToken token = default) =>
        await _rematchCancelledChannel.Reader.ReadAsync(
            token.WithTimeout(TimeSpan.FromSeconds(10))
        );

    public async Task<GameToken> GetNextRematchAcceptedAsync(CancellationToken token = default) =>
        await _rematchAcceptedChannel.Reader.ReadAsync(token.WithTimeout(TimeSpan.FromSeconds(10)));
}
