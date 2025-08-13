using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class LobbyHubClient : IAsyncDisposable
{
    public const string Path = "/api/hub/lobby";

    private readonly HubConnection _connection;
    private readonly TaskCompletionSource<string> _matchTsc = new();
    private readonly TaskCompletionSource<IEnumerable<SignalRError>> _errorTsc = new();

    public LobbyHubClient(HubConnection connection)
    {
        _connection = connection;
        _connection.On<string>(
            "MatchFoundAsync",
            gameToken =>
            {
                _matchTsc.TrySetResult(gameToken);
            }
        );
        _connection.On<IEnumerable<SignalRError>>(
            "ReceiveErrorAsync",
            errors => _errorTsc.TrySetResult(errors)
        );
    }

    public Task SeekRatedAsync(TimeControlSettings timeControl, CancellationToken token) =>
        _connection.InvokeAsync("SeekRatedAsync", timeControl, token);

    public Task SeekCasualAsync(TimeControlSettings timeControl, CancellationToken token) =>
        _connection.InvokeAsync("SeekCasualAsync", timeControl, token);

    public Task CancelSeekAsync(CancellationToken token) =>
        _connection.InvokeAsync("CancelSeekAsync", token);

    public Task<string> WaitForGameAsync(CancellationToken token) =>
        _matchTsc.Task.WaitAsync(TimeSpan.FromSeconds(10), token);

    public Task<IEnumerable<SignalRError>> WaitForErrorAsync(CancellationToken token) =>
        _errorTsc.Task.WaitAsync(TimeSpan.FromSeconds(10), token);

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _connection.DisposeAsync();
    }
}
