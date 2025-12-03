using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;

namespace AnarchyChess.Api.Functional.Tests.ProfileControllerTests;

public class GetUserTests(AnarchyChessWebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetSessionUser_with_an_authed_user()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;

        var response = await ApiClient.Api.GetSessionUserAuthedAsync();

        response.IsSuccessful.Should().BeTrue();

        PrivateUser expectedUser = new(
            UserId: user.Id,
            UserName: user.UserName!,
            About: user.About,
            CountryCode: user.CountryCode,
            CreatedAt: user.CreatedAt,
            UsernameLastChanged: user.UsernameLastChanged
        );
        response.Content.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetSessionUser_with_a_guest_user()
    {
        var guestId = UserId.Guest();
        AuthUtils.AuthenticateGuest(ApiClient, guestId);

        var response = await ApiClient.Api.GetSessionUserGuestAsync();

        response.IsSuccessful.Should().BeTrue();
        GuestUser expectedUser = new(guestId);
        response.Content.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetUserByUsername_with_an_existing_user()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetUserByUsernameAsync(user.UserName!);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetUserByUsername_with_a_non_existing_user()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetUserByUsernameAsync("wrong username doesn't exist");

        response.IsSuccessful.Should().BeFalse();
        response.Content.Should().BeNull();
    }

    [Fact]
    public async Task GetUserById_with_an_existing_user()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetUserByIdAsync(user.Id);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetUserById_with_a_non_existing_user()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetUserByIdAsync("non-existing-user-id");

        response.IsSuccessful.Should().BeFalse();
        response.Content.Should().BeNull();
    }
}
