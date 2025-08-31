using Chess2.Api.Pagination.Models;
using Chess2.Api.Social.Repository;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.SocialTests;

public class FriendRepositoryTests : BaseIntegrationTest
{
    private readonly IFriendRepository _repository;

    public FriendRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IFriendRepository>();
    }

    [Fact]
    public async Task GetIncomingFriendRequestsAsync_returns_correct_requests()
    {
        var recipient = new AuthedUserFaker().Generate();

        var request1 = new FriendRequestFaker(recipient: recipient).Generate();
        var request2 = new FriendRequestFaker(recipient: recipient).Generate();

        await DbContext.AddRangeAsync(recipient, request1, request2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetIncomingFriendRequestsAsync(
            recipient.Id,
            new PaginationQuery(Page: 0, PageSize: 20),
            CT
        );

        result.Should().BeEquivalentTo([request1.Requester, request2.Requester]);
    }

    [Fact]
    public async Task GetIncomingFriendRequestsAsync_applies_pagination()
    {
        var recipient = new AuthedUserFaker().Generate();
        var requests = new FriendRequestFaker(recipient: recipient).Generate(5);

        await DbContext.AddRangeAsync(recipient);
        await DbContext.AddRangeAsync(requests, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetIncomingFriendRequestsAsync(
            recipient.Id,
            new PaginationQuery(Page: 1, PageSize: 2),
            CT
        );

        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(requests.Skip(2).Take(2).Select(r => r.Requester));
    }

    [Fact]
    public async Task GetIncomingFriendRequestCount_returns_correct_number()
    {
        var recipient = new AuthedUserFaker().Generate();
        var requests = new FriendRequestFaker(recipient: recipient).Generate(5);
        var otherUserRequest = new FriendRequestFaker().Generate();

        await DbContext.AddRangeAsync(requests, CT);
        await DbContext.AddRangeAsync(recipient, otherUserRequest);
        await DbContext.SaveChangesAsync(CT);

        var count = await _repository.GetIncomingFriendRequestCount(recipient.Id, CT);

        count.Should().Be(requests.Count);
    }

    [Fact]
    public async Task GetRequestBetweenAsync_returns_request_between_users()
    {
        var request = new FriendRequestFaker().Generate();
        var otherRequest = new FriendRequestFaker().Generate();

        await DbContext.AddRangeAsync(request, otherRequest);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetRequestBetweenAsync(
            request.RequesterUserId,
            request.RecipientUserId,
            CT
        );

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(request);
    }

    [Fact]
    public async Task GetRequestBetweenAsync_returns_null_if_no_request_exists()
    {
        var request = new FriendRequestFaker().Generate();
        await DbContext.AddAsync(request.Requester, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetRequestBetweenAsync(
            request.RequesterUserId,
            "nonexistent-user",
            CT
        );

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddFriendRequestAsync_adds_request_to_db_context()
    {
        var request = new FriendRequestFaker().Generate();

        await _repository.AddFriendRequestAsync(request, CT);
        await DbContext.SaveChangesAsync(CT);

        var dbRequest = await DbContext.FriendRequests.AsNoTracking().SingleOrDefaultAsync(CT);

        dbRequest.Should().NotBeNull();
        dbRequest.Should().BeEquivalentTo(request);
    }

    [Fact]
    public async Task AddFriendAsync_adds_friend_to_db_context()
    {
        var friend = new FriendFaker().Generate();

        await _repository.AddFriendAsync(friend, CT);
        await DbContext.SaveChangesAsync(CT);

        var dbFriend = await DbContext.Friends.AsNoTracking().SingleOrDefaultAsync(CT);

        dbFriend.Should().NotBeNull();
        dbFriend.Should().BeEquivalentTo(friend);
    }

    [Fact]
    public async Task DeleteFriendRequest_removes_request_from_db_context()
    {
        var requestToDelete = new FriendRequestFaker().Generate();
        var otherRequest = new FriendRequestFaker().Generate();

        await DbContext.AddRangeAsync(requestToDelete, otherRequest);
        await DbContext.SaveChangesAsync(CT);

        _repository.DeleteFriendRequest(requestToDelete);
        await DbContext.SaveChangesAsync(CT);

        var dbRequest = await DbContext.FriendRequests.AsNoTracking().SingleOrDefaultAsync(CT);
        dbRequest.Should().BeEquivalentTo(otherRequest);
    }

    [Fact]
    public async Task GetFriendBetweenAsync_returns_friend_if_exists()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        var friend = new FriendFaker(user1, user2).Generate();

        await DbContext.AddRangeAsync(user1, user2, friend);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetFriendBetweenAsync(user1.Id, user2.Id, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(friend);
    }

    [Fact]
    public async Task GetFriendBetweenAsync_returns_friend_if_order_is_reversed()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        var friend = new FriendFaker(user1, user2).Generate();

        await DbContext.AddRangeAsync(user1, user2, friend);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetFriendBetweenAsync(user2.Id, user1.Id, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(friend);
    }

    [Fact]
    public async Task GetFriendBetweenAsync_returns_null_if_no_friend_exists()
    {
        var user1 = new AuthedUserFaker().Generate();
        var user2 = new AuthedUserFaker().Generate();
        await DbContext.AddRangeAsync(user1, user2);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetFriendBetweenAsync(user1.Id, user2.Id, CT);

        result.Should().BeNull();
    }
}
