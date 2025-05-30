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
    IAuthService authService
) : Chess2Hub<IMatchmakingClient>
{
    private readonly IMatchmakingService _matchmakingService = matchmakingService;
    private readonly ILogger<MatchmakingHub> _logger = logger;
    private readonly IAuthService _authService = authService;

    public async Task SeekRatedAsync(int baseMinutes, int increment)
    {
        var userId = Context.UserIdentifier;
        if (userId is null)
        {
            await HandleErrors(Error.Unauthorized());
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

    public async Task SeekCasualAsync(int baseMinutes, int increment)
    {
        var userId = Context.UserIdentifier;
        if (userId is null)
        {
            await HandleErrors(Error.Unauthorized());
            return;
        }

        _matchmakingService.SeekCasual(userId, baseMinutes, increment);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            _matchmakingService.CancelSeek(userId);
        return Task.CompletedTask;
    }
}
