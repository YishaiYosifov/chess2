using System.Net;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.Profile.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Functional.Tests.ProfileControllerTests;

public class EditProfileTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task EditProfile_modifies_the_user_when_provided_with_valid_data()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;
        var profileEdit = new ProfileEditRequest(About: user.About, CountryCode: "US");

        var response = await ApiClient.Api.EditProfileAsync(profileEdit);

        response.IsSuccessful.Should().BeTrue();
        var updatedUser = await DbContext.Users.AsNoTracking().SingleAsync(CT);
        updatedUser.CountryCode.Should().NotBe(user.CountryCode);
        updatedUser.CountryCode.Should().Be(profileEdit.CountryCode);
    }

    [Fact]
    public async Task EditProfile_rejects_invalid_data()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;
        var profileEdit = new ProfileEditRequest(About: user.About, CountryCode: "XZ");

        var response = await ApiClient.Api.EditProfileAsync(profileEdit);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync(CT);
        dbUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task EditUsername_changes_the_username_when_provided_with_valid_data()
    {
        var newUsername = "new-test-username";
        var user = new AuthedUserFaker()
            .RuleFor(x => x.UsernameLastChanged, DateTime.UtcNow - TimeSpan.FromDays(365))
            .Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user);

        var response = await ApiClient.Api.EditUsernameAsync(new(newUsername));

        response.IsSuccessful.Should().BeTrue();
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync(CT);
        dbUser.UserName.Should().Be(newUsername);
    }
}
