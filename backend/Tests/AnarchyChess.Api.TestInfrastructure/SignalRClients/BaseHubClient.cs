using System.Threading.Channels;
using AnarchyChess.Api.Infrastructure.SignalR;
using AnarchyChess.Api.TestInfrastructure.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace AnarchyChess.Api.TestInfrastructure.SignalRClients;

public abstract class BaseHubClient : IAsyncDisposable
{
    protected HubConnection Connection { get; }

    private readonly Channel<IEnumerable<SignalRError>> _errorChannel = Channel.CreateUnbounded<
        IEnumerable<SignalRError>
    >();

    public BaseHubClient(HubConnection connection)
    {
        Connection = connection;

        Connection.On<IEnumerable<SignalRError>>(
            "ReceiveErrorAsync",
            errors => _errorChannel.Writer.TryWrite(errors)
        );
    }

    public Task StartAsync(CancellationToken token = default) => Connection.StartAsync(token);

    public Task StopAsync(CancellationToken token = default) => Connection.StopAsync(token);

    public async Task<IEnumerable<SignalRError>> GetNextErrorsAsync(CancellationToken token)
    {
        using var cts = token.WithTimeout(TimeSpan.FromSeconds(10));
        var errors = await _errorChannel.Reader.ReadAsync(cts.Token);
        return errors;
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
