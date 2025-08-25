using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.DTOs;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.ProfileControllerTests;

public class GetUserTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetSessionUser_with_an_authed_user()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;

        var response = await ApiClient.Api.GetSessionUserAuthedAsync();

        response.IsSuccessful.Should().BeTrue();

        PrivateUser expectedUser = new(
            UserId: user.Id,
            UsernameLastChangedSeconds: user.UsernameLastChanged is null
                ? null
                : new DateTimeOffset(user.UsernameLastChanged.Value).ToUnixTimeSeconds(),
            UserName: user.UserName!,
            About: user.About,
            CountryCode: user.CountryCode
        );
        response.Content.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetSessionUser_with_a_guest_user()
    {
        const string guestId = "guest 123";
        AuthUtils.AuthenticateGuest(ApiClient, guestId);

        var response = await ApiClient.Api.GetSessionUserGuestAsync();

        response.IsSuccessful.Should().BeTrue();
        GuestUser expectedUser = new(guestId);
        response.Content.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetUser_with_an_existing_user()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var response = await ApiClient.Api.GetUserAsync(user.UserName!);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetUser_with_a_non_existing_user()
    {
        await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var response = await ApiClient.Api.GetUserAsync("wrong username doesn't exist");

        response.IsSuccessful.Should().BeFalse();
        response.Content.Should().BeNull();
    }
}
