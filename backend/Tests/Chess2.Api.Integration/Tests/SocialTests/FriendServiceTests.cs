using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Social.Errors;
using Chess2.Api.Social.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.SocialTests;

public class FriendServiceTests : BaseIntegrationTest
{
    private readonly IFriendService _friendService;

    public FriendServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _friendService = Scope.ServiceProvider.GetRequiredService<IFriendService>();
    }

    [Fact]
    public async Task GetFriendRequestsAsync_returns_minimal_profiles()
    {
        var recipient = new AuthedUserFaker().Generate();
        var requests = new FriendRequestFaker(recipient: recipient).Generate(3);

        await DbContext.AddAsync(recipient, CT);
        await DbContext.AddRangeAsync(requests, CT);
        await DbContext.SaveChangesAsync(CT);

        var pagination = new PaginationQuery(Page: 0, PageSize: 20);
        var result = await _friendService.GetFriendRequestsAsync(recipient.Id, pagination, CT);

        result
            .Items.Should()
            .BeEquivalentTo(
                requests.Select(r => new MinimalProfile(
                    UserId: r.Requester.Id,
                    UserName: r.Requester.UserName!
                ))
            );
        result.TotalCount.Should().Be(requests.Count);
    }

    [Fact]
    public async Task GetFriendRequestsAsync_applies_pagination()
    {
        var recipient = new AuthedUserFaker().Generate();
        var requests = new FriendRequestFaker(recipient: recipient).Generate(5);

        await DbContext.AddAsync(recipient, CT);
        await DbContext.AddRangeAsync(requests, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _friendService.GetFriendRequestsAsync(
            recipient.Id,
            new PaginationQuery(Page: 1, PageSize: 2),
            CT
        );

        result.Items.Should().HaveCount(2);
        result
            .Items.Should()
            .BeEquivalentTo(
                requests
                    .Skip(2)
                    .Take(2)
                    .Select(r => new MinimalProfile(
                        UserId: r.Requester.Id,
                        UserName: r.Requester.UserName ?? "Unknown"
                    ))
            );
        result.TotalCount.Should().Be(requests.Count);
    }

    [Fact]
    public async Task RequestFriendAsync_creates_new_request_if_none_exists()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        var recipientPrefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.AllowFriendRequests, true)
            .Generate();

        await DbContext.AddRangeAsync(requester, recipient, recipientPrefs);
        await DbContext.SaveChangesAsync(CT);

        var result = await _friendService.RequestFriendAsync(requester, recipient, CT);

        result.IsError.Should().BeFalse();

        var dbRequest = await DbContext.FriendRequests.AsNoTracking().SingleOrDefaultAsync(CT);
        dbRequest.Should().NotBeNull();
        dbRequest.RequesterUserId.Should().Be(requester.Id);
        dbRequest.RecipientUserId.Should().Be(recipient.Id);
    }

    [Fact]
    public async Task RequestFriendAsync_returns_error_if_recipient_not_accepting()
    {
        var requester = new AuthedUserFaker().Generate();
        var recipient = new AuthedUserFaker().Generate();
        var recipientPrefs = new UserPreferencesFaker(recipient)
            .RuleFor(x => x.AllowFriendRequests, false)
            .Generate();

        await DbContext.AddRangeAsync(requester, recipient, recipientPrefs);
        await DbContext.SaveChangesAsync(CT);

        var result = await _friendService.RequestFriendAsync(requester, recipient, CT);

        result.FirstError.Should().Be(SocialErrors.NotAcceptingFriends);
    }

    [Fact]
    public async Task RequestFriendAsync_accepts_existing_request_if_found()
    {
        var existingRequest = new FriendRequestFaker().Generate();
        await DbContext.AddAsync(existingRequest, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _friendService.RequestFriendAsync(
            existingRequest.Recipient,
            existingRequest.Requester,
            CT
        );

        result.IsError.Should().BeFalse();

        var dbFriend = await DbContext.Friends.AsNoTracking().SingleOrDefaultAsync(CT);
        dbFriend.Should().NotBeNull();
        dbFriend.UserId1.Should().Be(existingRequest.Recipient.Id);
        dbFriend.UserId2.Should().Be(existingRequest.Requester.Id);

        var dbRequests = await DbContext.FriendRequests.AsNoTracking().ToListAsync(CT);
        dbRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task RequestFriendAsync_returns_error_if_request_already_sent_by_requester()
    {
        var existingRequest = new FriendRequestFaker().Generate();
        await DbContext.AddAsync(existingRequest, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _friendService.RequestFriendAsync(
            existingRequest.Requester,
            existingRequest.Recipient,
            CT
        );

        result.FirstError.Should().Be(SocialErrors.FriendAlreadyRequested);
    }
}
