using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Models;
using Chess2.Api.Social.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace Chess2.Api.Social.Services;

public interface ISocialNotifier
{
    Task NotifyFriendRequest(UserId recipientId, MinimalProfile requester);
    Task NotifyFriendRequestAccepted(UserId requesterId, UserId recipientId);
    Task NotifyFriendRequestRemoved(UserId requesterId, UserId recipientId);
}

public class SocialNotifier(IHubContext<SocialHub, ISocialHubClient> hub) : ISocialNotifier
{
    private readonly IHubContext<SocialHub, ISocialHubClient> _hub = hub;

    public Task NotifyFriendRequest(UserId recipientId, MinimalProfile requester) =>
        _hub.Clients.User(recipientId).NewFriendRequest(requester);

    public async Task NotifyFriendRequestAccepted(UserId requesterId, UserId recipientId)
    {
        await _hub.Clients.User(requesterId).FriendRequestAccepted(requesterId, recipientId);
        await _hub.Clients.User(recipientId).FriendRequestAccepted(requesterId, recipientId);
    }

    public async Task NotifyFriendRequestRemoved(UserId requesterId, UserId recipientId)
    {
        await _hub.Clients.User(requesterId).FriendRequestRemoved(requesterId, recipientId);
        await _hub.Clients.User(recipientId).FriendRequestRemoved(requesterId, recipientId);
    }
}
