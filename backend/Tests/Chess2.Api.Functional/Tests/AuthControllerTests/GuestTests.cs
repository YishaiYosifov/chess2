using Chess2.Api.TestInfrastructure;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests.AuthControllerTests;

public class GuestTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task Guest_token_is_created_succesfully()
    {
        await AuthUtils.AssertGuestUnauthenticated(ApiClient);

        var response = await ApiClient.Api.CreateGuestAsync();

        response.IsSuccessful.Should().BeTrue();
        await AuthUtils.AssertGuestAuthenticated(ApiClient);
        await AuthUtils.AssertUnauthenticated(ApiClient);
    }
}
