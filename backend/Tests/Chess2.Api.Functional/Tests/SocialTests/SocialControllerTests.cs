using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Chess2.Api.Functional.Tests.SocialTests;

public class SocialControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetStarredUsers_returns_expected_paginated_stars()
    {
        var user = new AuthedUserFaker().Generate();
        var stars = new StarredUserFaker(forUser: user.Id).Generate(5);

        await DbContext.AddAsync(user, CT);
        await DbContext.AddRangeAsync(stars, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetStarredUsersAsync(
            user.Id,
            new PaginationQuery(Page: 1, PageSize: 2)
        );

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        response
            .Content.Items.Should()
            .BeEquivalentTo(stars.Skip(2).Take(2).Select(x => new MinimalProfile(x.Starred)));
        response.Content.TotalCount.Should().Be(stars.Count);
    }

    [Fact]
    public async Task GetStarredUsers_returns_bad_request_for_invalid_pagination()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetStarredUsersAsync(
            user.Id,
            new PaginationQuery(Page: 0, PageSize: -1)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStarsReceivedCount_returns_expected_count()
    {
        var starredUser = new AuthedUserFaker().Generate();
        var stargazers = new StarredUserFaker(starredUser: starredUser).Generate(3);

        await DbContext.AddAsync(starredUser, CT);
        await DbContext.AddRangeAsync(stargazers, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetStarsReceivedCountAsync(starredUser.Id);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().Be(3);
    }

    [Fact]
    public async Task IsStarred_returns_true_when_user_has_starred()
    {
        var user = new AuthedUserFaker().Generate();
        var star = new StarredUserFaker(forUser: user.Id).Generate();

        await DbContext.AddRangeAsync(user, star);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user);

        var response = await ApiClient.Api.GetIsStarredAsync(star.StarredUserId);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeTrue();
    }

    [Fact]
    public async Task IsStarred_returns_false_when_user_has_not_starred()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user);

        var response = await ApiClient.Api.GetIsStarredAsync("some random user");

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeFalse();
    }

    [Fact]
    public async Task AddStar_returns_no_content_when_successful()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;
        var starredUser = new AuthedUserFaker().Generate();

        await DbContext.AddAsync(starredUser, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.AddStarAsync(starredUser.Id);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbStar = await DbContext.StarredUsers.AsNoTracking().SingleOrDefaultAsync(CT);
        dbStar.Should().NotBeNull();
        dbStar.UserId.Should().Be(user.Id);
        dbStar.StarredUserId.Should().Be(starredUser.Id);
    }

    [Fact]
    public async Task AddStar_returns_not_found_if_starred_user_does_not_exist()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var response = await ApiClient.Api.AddStarAsync("non-existent-user-id");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveStar_returns_no_content_when_successful()
    {
        var user = new AuthedUserFaker().Generate();
        var star = new StarredUserFaker(forUser: user.Id).Generate();
        await DbContext.AddRangeAsync(user, star);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user);

        var response = await ApiClient.Api.RemoveStarAsync(star.StarredUserId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dbStar = await DbContext.StarredUsers.AsNoTracking().SingleOrDefaultAsync(CT);
        dbStar.Should().BeNull();
    }

    [Fact]
    public async Task RemoveStar_returns_not_found_if_star_does_not_exist()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var response = await ApiClient.Api.RemoveStarAsync("non-existent-user-id");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
