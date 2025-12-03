using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;

namespace AnarchyChess.Api.Functional.Tests.AuthControllerTests;

public class GuestTests(AnarchyChessWebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task CreateGuest_generates_and_authenticates_a_guest_successfully()
    {
        await AuthTestUtils.AssertGuestUnauthenticated(ApiClient);

        var response = await ApiClient.Api.CreateGuestAsync();

        response.IsSuccessful.Should().BeTrue();
        await AuthTestUtils.AssertGuestAuthenticated(ApiClient);
        await AuthTestUtils.AssertUnauthenticated(ApiClient);
    }
}
