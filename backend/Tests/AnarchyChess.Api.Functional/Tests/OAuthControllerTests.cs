using System.Net;
using System.Web;
using AnarchyChess.Api.TestInfrastructure;
using AwesomeAssertions;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace AnarchyChess.Api.Functional.Tests;

public class OAuthControllerTests(AnarchyChessWebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Theory]
    [InlineData(Providers.Google, "accounts.google.com", "/o/oauth2/v2/auth")]
    [InlineData(Providers.Discord, "discord.com", "/oauth2/authorize")]
    public async Task SignInOAuth_redirects_to_the_correct_place(
        string provider,
        string host,
        string pathname
    )
    {
        var response = await ApiClient.Api.SignInOAuthAsync(provider);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var redirectUrl = response.Headers.Location!;
        redirectUrl.Host.Should().Be(host);
        redirectUrl.AbsolutePath.Should().Be(pathname);

        var redirectBackUrl = new Uri(
            HttpUtility.ParseQueryString(redirectUrl.Query).Get("redirect_uri")!
        );
        redirectBackUrl.AbsolutePath.Should().Be($"/api/oauth/{provider.ToLower()}/callback");
    }

    [Fact]
    public async Task SignInOAuth_returns_not_found_for_invalid_provider()
    {
        var response = await ApiClient.Api.SignInOAuthAsync("unknown");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
