using Microsoft.AspNetCore.SignalR.Client;

namespace Chess2.Api.TestInfrastructure.SignalRClients;

public class OpenSeekHubClient : BaseHubClient
{
    private readonly TaskCompletionSource<IEnumerable<OpenSeek>>

    public OpenSeekHubClient(HubConnection connection)
        : base(connection) { }
}
