using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Models;
using Chess2.Api.Social.Services;
using Chess2.Api.Social.SignalR;
using Chess2.Api.TestInfrastructure.Fakes;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.SocialTests;

public class SocialNotifierTests
{
    private readonly UserId _user1 = "user1";
    private readonly UserId _user2 = "user2";

    private readonly IHubContext<SocialHub, ISocialHubClient> _hubContextMock = Substitute.For<
        IHubContext<SocialHub, ISocialHubClient>
    >();
    private readonly IHubClients<ISocialHubClient> _clientsMock = Substitute.For<
        IHubClients<ISocialHubClient>
    >();
    private readonly ISocialHubClient _user1ClientMock = Substitute.For<ISocialHubClient>();
    private readonly ISocialHubClient _user2ClientMock = Substitute.For<ISocialHubClient>();

    private readonly SocialNotifier _notifier;

    public SocialNotifierTests()
    {
        _clientsMock.User(_user1).Returns(_user1ClientMock);
        _clientsMock.User(_user2).Returns(_user2ClientMock);
        _hubContextMock.Clients.Returns(_clientsMock);

        _notifier = new SocialNotifier(_hubContextMock);
    }

    [Fact]
    public async Task NotifyFriendRequest_calls_NewFriendRequest_on_recipient_only()
    {
        MinimalProfile profile = new(
            new AuthedUserFaker().RuleFor(x => x.Id, _user1.Value).Generate()
        );
        await _notifier.NotifyFriendRequest(_user2, profile);

        await _user2ClientMock.Received(1).NewFriendRequest(profile);
        await _user1ClientMock.DidNotReceiveWithAnyArgs().NewFriendRequest(default!);
    }

    [Fact]
    public async Task NotifyFriendRequestAccepted_calls_FriendRequestAccepted_on_both_users()
    {
        await _notifier.NotifyFriendRequestAccepted(_user1, _user2);

        await _user1ClientMock.Received(1).FriendRequestAccepted(_user1, _user2);
        await _user2ClientMock.Received(1).FriendRequestAccepted(_user1, _user2);
    }

    [Fact]
    public async Task NotifyFriendRequestRemoved_calls_FriendRequestRemoved_on_both_users()
    {
        await _notifier.NotifyFriendRequestRemoved(_user1, _user2);

        await _user1ClientMock.Received(1).FriendRequestRemoved(_user1, _user2);
        await _user2ClientMock.Received(1).FriendRequestRemoved(_user1, _user2);
    }
}
