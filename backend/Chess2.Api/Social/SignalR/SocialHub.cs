using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.Social.SignalR;

public interface ISocialHubClient : IChess2HubClient { }

[Authorize(AuthPolicies.ActiveSession)]
public class SocialHub : Chess2Hub<ISocialHubClient>;
