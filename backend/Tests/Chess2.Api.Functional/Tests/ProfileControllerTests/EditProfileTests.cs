using System.Net;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Functional.Tests.ProfileControllerTests;

public class EditProfileTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task EditProfile_modifies_the_user_when_provided_with_valid_data()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;
        var profileEdit = new JsonPatchDocument<ProfileEditRequest>();
        profileEdit.Replace(profileEdit => profileEdit.CountryCode, "US");

        var response = await ApiClient.Api.EditProfileAsync(profileEdit);

        response.IsSuccessful.Should().BeTrue();
        var updatedUser = await DbContext.Users.AsNoTracking().SingleAsync();
        updatedUser.CountryCode.Should().NotBe(user.CountryCode);
        updatedUser.CountryCode.Should().Be("US");
    }

    [Fact]
    public async Task EditProfile_rejects_invalid_data()
    {
        var user = (await AuthUtils.AuthenticateAsync(ApiClient)).User;
        var invalidProfileEdit = new JsonPatchDocument<ProfileEditRequest>();
        invalidProfileEdit.Replace(profileEdit => profileEdit.CountryCode, "XZ");

        var response = await ApiClient.Api.EditProfileAsync(invalidProfileEdit);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync();
        dbUser.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task EditUsername_changes_the_username_when_provided_with_valid_data()
    {
        var newUsername = "new-test-username";
        var user = await FakerUtils.StoreFakerAsync(
            DbContext,
            new AuthedUserFaker().RuleFor(
                x => x.UsernameLastChanged,
                DateTime.UtcNow - TimeSpan.FromDays(365)
            )
        );
        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user);

        var response = await ApiClient.Api.EditUsernameAsync($"\"{newUsername}\"");

        response.IsSuccessful.Should().BeTrue();
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync();
        dbUser.UserName.Should().Be(newUsername);
    }
}
