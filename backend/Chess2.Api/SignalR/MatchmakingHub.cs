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

[Authorize("AccessToken")]
public class MatchmakingHub(ILogger<MatchmakingHub> logger, IMatchmakingService matchmakingService, IAuthService authService) : Hub<IMatchmakingClient>
{
    private readonly IMatchmakingService _matchmakingService = matchmakingService;
    private readonly ILogger<MatchmakingHub> _logger = logger;
    private readonly IAuthService _authService = authService;

    public async Task SeekMatchAsync()
    {   
        var userResult = await _authService.GetLoggedInUserAsync(Context);
        if (userResult.IsError)
        {
            await Clients.Caller.ReceiveError(userResult.Errors.ToSignalR());
            return;
        }

        var user = userResult.Value;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        
    }
}
