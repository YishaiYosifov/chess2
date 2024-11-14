using Chess2.Api.Integration.Utils;
using FluentAssertions;

namespace Chess2.Api.Integration.Tests;

public class UserControllerTests(Chess2WebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Get_authenticated_user()
    {
        var user = await AuthTestUtils.Authenticate(ApiClient, DbContext);

        var response = await ApiClient.GetAuthedUser();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().BeEquivalentTo(
            user, opts => opts.ExcludingMissingMembers());
    }
}
