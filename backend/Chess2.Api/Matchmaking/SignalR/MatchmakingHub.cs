using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.Matchmaking.Services;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.Matchmaking.SignalR;

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

    public async Task SeekMatchAsync(int baseMinutes, int increment)
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
            _matchmakingService.SeekCasual(userId, baseMinutes, increment);
            return;
        }

        var userResult = await _authService.GetLoggedInUserAsync(Context.User);
        if (userResult.IsError)
        {
            await HandleErrors(userResult.Errors);
            return;
        }
        await _matchmakingService.SeekRatedAsync(userResult.Value, baseMinutes, increment);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            _matchmakingService.CancelSeek(userId);
        return Task.CompletedTask;
    }
}
