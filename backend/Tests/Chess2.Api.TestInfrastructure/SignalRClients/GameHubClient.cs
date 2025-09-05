using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

using ChatTcs = TaskCompletionSource<(string senderUserId, string senderUserName, string message)>;

public class GameHubClient : BaseHubClient
{
    public static string Path(string gameToken) => $"/api/hub/game?gameToken={gameToken}";

    private readonly ChatTcs _messageTcs = new();
    private readonly TaskCompletionSource _connectedTcs = new();
    private readonly string _gameToken;

    public GameHubClient(HubConnection connection, string gameToken)
        : base(connection)
    {
        _gameToken = gameToken;

        Connection.On("ChatConnectedAsync", _connectedTcs.SetResult);
        Connection.On<string, string, string>(
            "ChatMessageAsync",
            (senderUserId, senderUserName, message) =>
                _messageTcs.TrySetResult((senderUserId, senderUserName, message))
        );
    }

    public static async Task<GameHubClient> ConnectToChatAsync(
        HubConnection connection,
        string gameToken,
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

    public Task<(string SenderUserId, string SenderUserName, string Message)> WaitForMessageAsync(
        CancellationToken token
    ) => _messageTcs.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
}
