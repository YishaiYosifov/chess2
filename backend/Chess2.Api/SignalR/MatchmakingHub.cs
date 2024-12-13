using Chess2.Api.Extensions;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.SignalR;

public interface IMatchmakingClient
{
    public Task MatchFound(string token);
    public Task ReceiveError(IEnumerable<SignalRError> error);
}

[Authorize("GuestAccess")]
public class MatchmakingHub(
    ILogger<MatchmakingHub> logger,
    IMatchmakingService matchmakingService,
    IGuestService guestService
) : Hub<IMatchmakingClient>
{
    private readonly IMatchmakingService _matchmakingService = matchmakingService;
    private readonly ILogger<MatchmakingHub> _logger = logger;
    private readonly IGuestService _guestService = guestService;

    public async Task SeekMatchAsync()
    {
        var anonUserResult = _guestService.GetAnonUserAsync(Context.User);
        if (anonUserResult.IsError)
        {
            await Clients.Caller.ReceiveError(anonUserResult.Errors.ToSignalR());
            return;
        }
        var anonUser = anonUserResult.Value;

        await _matchmakingService.Seek(anonUser.UserId, 1, 2, 3);
    }

    public override async Task OnDisconnectedAsync(Exception? exception) { }
}
