using Chess2.Api.Integration.Fakes;
using Chess2.Api.Integration.Utils;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests;

public class UserControllerTests(Chess2WebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Get_authenticated_user()
    {
        var user = await AuthTestUtils.Authenticate(ApiClient, DbContext);

        var response = await ApiClient.GetAuthedUserAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(
            user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Get_user()
    {
        var user = await FakerUtils.StoreFaker(DbContext, new UserFaker());

        var response = await ApiClient.GetUserAsync(user.Username);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(
            user, opts => opts.ExcludingMissingMembers());
    }

    [Fact]
    public async Task Get_non_existing_user()
    {
        var user = await FakerUtils.StoreFaker(DbContext, new UserFaker());

        var response = await ApiClient.GetUserAsync("wrong username doesn't exist");

        response.IsSuccessful.Should().BeFalse();
        response.Content.Should().BeNull();
    }
}
