using System.Net;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Functional.Tests.SocialTests;

public class SocialControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetFriends_returns_expected_paginated_friends()
    {
        var recipient = new AuthedUserFaker().Generate();
        var requests = new FriendRequestFaker(recipient: recipient).Generate(5);
        await DbContext.AddAsync(recipient, CT);
        await DbContext.AddRangeAsync(requests, CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, recipient);

        var response = await ApiClient.Api.GetFriendsAsync(
            new PaginationQuery(Page: 1, PageSize: 2)
        );

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        response
            .Content.Items.Should()
            .BeEquivalentTo(
                requests
                    .Skip(2)
                    .Take(2)
                    .Select(r => new MinimalProfile(
                        UserId: r.Requester.Id,
                        UserName: r.Requester.UserName!
                    ))
            );
    }

    [Fact]
    public async Task GetFriends_returns_bad_request_for_invalid_pagination()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var response = await ApiClient.Api.GetFriendsAsync(
            new PaginationQuery(Page: 0, PageSize: -1)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RequestFriend_returns_no_content_when_successful()
    {
        var requester = (await AuthUtils.AuthenticateAsync(ApiClient)).User;
        var recipient = new AuthedUserFaker().Generate();

        await DbContext.AddAsync(recipient, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.RequestFriendAsync(recipient.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbRequest = await DbContext.FriendRequests.AsNoTracking().SingleOrDefaultAsync(CT);
        dbRequest.Should().NotBeNull();
        dbRequest.RequesterUserId.Should().Be(requester.Id);
        dbRequest.RecipientUserId.Should().Be(recipient.Id);
    }

    [Fact]
    public async Task RequestFriend_returns_not_found_if_recipient_does_not_exist()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var response = await ApiClient.Api.RequestFriendAsync("random user id");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
