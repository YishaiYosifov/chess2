using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.SignalR;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Models;
using Microsoft.AspNetCore.Authorization;

namespace Chess2.Api.Social.SignalR;

public interface ISocialHubClient : IChess2HubClient
{
    Task NewFriendRequest(MinimalProfile by);
    Task FriendRequestAccepted(UserId requesterId, UserId recipientId);
    Task FriendRequestRemoved(UserId requesterId, UserId recipientId);
}

[Authorize(AuthPolicies.ActiveSession)]
public class SocialHub : Chess2Hub<ISocialHubClient>;
