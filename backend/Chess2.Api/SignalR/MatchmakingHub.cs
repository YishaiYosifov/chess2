using Chess2.Api.Services;
using Chess2.Api.Services.Matchmaking;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.SignalR;

public interface IMatchmakingClient : IChess2HubClient
{
    public Task MatchFoundAsync(string token);
}

[Authorize("GuestAccess")]
public class MatchmakingHub(
    ILogger<MatchmakingHub> logger,
    IMatchmakingService matchmakingService,
    IGuestService guestService,
    IAuthService authService
) : Chess2Hub<IMatchmakingClient>
{
    private readonly IMatchmakingService _matchmakingService = matchmakingService;
    private readonly IGuestService _guestService = guestService;
    private readonly ILogger<MatchmakingHub> _logger = logger;
    private readonly IAuthService _authService = authService;

    public async Task SeekMatchAsync(int timeControl, int increment)
    {
        var userId = Context.UserIdentifier;
        if (userId is null)
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        var isGuest = _guestService.IsGuest(Context.User);
        if (isGuest)
        {
            await _matchmakingService.SeekGuestAsync(userId, timeControl, increment);
            return;
        }

        var userResult = await _authService.GetLoggedInUserAsync(Context.User);
        if (userResult.IsError)
        {
            await HandleErrors(userResult.Errors);
            return;
        }
        await _matchmakingService.SeekAsync(userResult.Value, timeControl, increment);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        throw new NotImplementedException();
    }
}
