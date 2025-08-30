using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class LobbyHubClient : BaseHubClient
{
    public const string Path = "/api/hub/lobby";

    private readonly TaskCompletionSource<string> _matchTsc = new();
    private readonly TaskCompletionSource<IEnumerable<SignalRError>> _errorTsc = new();

    public LobbyHubClient(HubConnection connection)
        : base(connection)
    {
        Connection.On<string>("MatchFoundAsync", gameToken => _matchTsc.TrySetResult(gameToken));
        Connection.On<IEnumerable<SignalRError>>(
            "ReceiveErrorAsync",
            errors => _errorTsc.TrySetResult(errors)
        );
    }

    public Task SeekRatedAsync(TimeControlSettings timeControl, CancellationToken token) =>
        Connection.InvokeAsync("SeekRatedAsync", timeControl, token);

    public Task SeekCasualAsync(TimeControlSettings timeControl, CancellationToken token) =>
        Connection.InvokeAsync("SeekCasualAsync", timeControl, token);

    public Task CancelSeekAsync(PoolKey pool, CancellationToken token) =>
        Connection.InvokeAsync("CancelSeekAsync", pool, token);

    public Task MatchWithOpenSeekAsync(UserId matchWith, PoolKey pool, CancellationToken token) =>
        Connection.InvokeAsync("MatchWithOpenSeekAsync", matchWith, pool, token);

    public Task<string> WaitForGameAsync(CancellationToken token) =>
        _matchTsc.Task.WaitAsync(TimeSpan.FromSeconds(10), token);

    public Task<IEnumerable<SignalRError>> WaitForErrorAsync(CancellationToken token) =>
        _errorTsc.Task.WaitAsync(TimeSpan.FromSeconds(10), token);
}
