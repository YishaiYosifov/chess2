using Chess2.Api.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.CodeAnalysis;

namespace Chess2.Api.Infrastructure.SignalR;

public interface IChess2HubClient
{
    Task ReceiveErrorAsync(IEnumerable<SignalRError> error);
}

public class Chess2Hub<T> : Hub<T>
    where T : class, IChess2HubClient
{
    protected bool TryGetUserId([NotNullWhen(true)] out string? userId)
    {
        userId = Context.UserIdentifier;
        return userId is not null;
    }

    protected Task HandleErrors(Error error) => HandleErrors([error]);

    protected async Task HandleErrors(IEnumerable<Error> errors)
    {
        await Clients.Caller.ReceiveErrorAsync(errors.ToSignalR());
    }
}
