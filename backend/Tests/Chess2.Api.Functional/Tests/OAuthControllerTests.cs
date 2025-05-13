using System.Net;
using System.Web;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Chess2.Api.Functional.Tests;

public class OAuthControllerTests(Chess2WebApplicationFactory factory) : BaseFunctionalTest(factory)
{
    [Theory]
    [InlineData(Providers.Google, "accounts.google.com", "/o/oauth2/v2/auth")]
    [InlineData(Providers.Discord, "discord.com", "/oauth2/authorize")]
    public async Task SigninOAuth_redirects_to_the_correct_place(
        string provider,
        string host,
        string pathname
    )
    {
        var response = await ApiClient.Api.OAuthLoginAsync(provider);

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
    public async Task OAuthCallback_ReturnsError_WhenProviderIsInvalid()
    {
        var response = await ApiClient.Api.OAuthLoginAsync("unknown");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
