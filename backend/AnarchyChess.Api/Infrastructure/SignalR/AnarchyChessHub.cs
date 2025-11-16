using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Api.ErrorHandling.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.SignalR;

namespace AnarchyChess.Api.Infrastructure.SignalR;

public interface IAnarchyChessHubClient
{
    Task ReceiveErrorAsync(IEnumerable<SignalRError> error);
}

public class AnarchyChessHub<T> : Hub<T>
    where T : class, IAnarchyChessHubClient
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
