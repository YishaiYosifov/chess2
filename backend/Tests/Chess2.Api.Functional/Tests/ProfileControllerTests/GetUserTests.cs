using System.Net;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.ProfileControllerTests;

public class GetUserTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task Get_authenticated_user()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;

        var response = await ApiClient.Api.GetAuthedUserAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Get_my_id_with_authed_user()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;

        var response = await ApiClient.Api.GetMyIdAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().Be(user.Id);
    }

    [Fact]
    public async Task Get_my_id_with_a_guest_user()
    {
        const string guestId = "guestid";
        AuthUtils.AuthenticateWithTokens(
            ApiClient,
            accessToken: TokenProvider.GenerateGuestToken(guestId)
        );

        var response = await ApiClient.Api.GetMyIdAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().Be(guestId);
    }

    [Fact]
    public async Task Get_my_id_when_not_authenticated()
    {
        var response = await ApiClient.Api.GetMyIdAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_user()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var response = await ApiClient.Api.GetUserAsync(user.UserName!);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Get_non_existing_user()
    {
        await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var response = await ApiClient.Api.GetUserAsync("wrong username doesn't exist");

        response.IsSuccessful.Should().BeFalse();
        response.Content.Should().BeNull();
    }
}
