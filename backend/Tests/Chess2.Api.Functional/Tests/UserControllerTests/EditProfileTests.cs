using Chess2.Api.Functional.Utils;
using Chess2.Api.Models.DTOs;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Chess2.Api.Functional.Tests.UserControllerTests;

public class EditProfileTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task Edit_profile_with_valid_data()
    {
        var user = (await AuthTestUtils.Authenticate(ApiClient, DbContext)).User;
        var profileEdit = new ProfileEdit() { CountryCode = "US" };

        var response = await ApiClient.EditProfileAsync(profileEdit);

        response.IsSuccessful.Should().BeTrue();
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync();
        dbUser.CountryCode.Should().NotBe(user.CountryCode);
        dbUser.CountryCode.Should().Be(profileEdit.CountryCode);
    }

    [Fact]
    public async Task Edit_profile_with_invalid_data()
    {
        var user = (await AuthTestUtils.Authenticate(ApiClient, DbContext)).User;
        var invalidProfileEdit = new ProfileEdit() { CountryCode = "XZ" };

        var response = await ApiClient.EditProfileAsync(invalidProfileEdit);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var dbUser = await DbContext.Users.AsNoTracking().SingleAsync();
        dbUser.Should().BeEquivalentTo(user);
    }
}
