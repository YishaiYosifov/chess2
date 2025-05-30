using Chess2.Api.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Infrastructure.SignalR;

public interface IChess2HubClient
{
    Task ReceiveErrorAsync(IEnumerable<SignalRError> error);
}

public class Chess2Hub<T> : Hub<T>
    where T : class, IChess2HubClient
{
    protected Task HandleErrors(Error error) => HandleErrors([error]);

    protected async Task HandleErrors(IEnumerable<Error> errors)
    {
        await Clients.Caller.ReceiveErrorAsync(errors.ToSignalR());
    }
}
