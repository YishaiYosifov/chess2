using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public abstract class BaseHubClient(HubConnection connection) : IAsyncDisposable
{
    protected HubConnection Connection { get; } = connection;

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
