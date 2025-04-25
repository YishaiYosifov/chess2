using Chess2.Api.Functional.Utils;
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
        var user = await AuthTestUtils.Authenticate(ApiClient, DbContext);

        var response = await ApiClient.GetAuthedUserAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Get_user()
    {
        var user = await FakerUtils.StoreFaker(DbContext, new AuthedUserFaker());

        var response = await ApiClient.GetUserAsync(user.Username);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Get_non_existing_user()
    {
        await FakerUtils.StoreFaker(DbContext, new AuthedUserFaker());

        var response = await ApiClient.GetUserAsync("wrong username doesn't exist");

        response.IsSuccessful.Should().BeFalse();
        response.Content.Should().BeNull();
    }
}
