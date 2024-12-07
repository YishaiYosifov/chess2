using AutoFixture;
using Chess2.Api.Models;
using Chess2.Api.Services;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests;

public class GuestServiceTests : BaseUnitTest
{
    private readonly IWebHostEnvironment _hostEnvironmentMock;
    private readonly IOptions<AppSettings> _appSettingsMock;
    private readonly ITokenProvider _tokenProviderMock;
    private readonly GuestService _guestService;

    public GuestServiceTests()
    {
        _hostEnvironmentMock = Fixture.Create<IWebHostEnvironment>();
        _appSettingsMock = Fixture.Create<IOptions<AppSettings>>();
        _tokenProviderMock = Fixture.Create<ITokenProvider>();
        _guestService = new(_tokenProviderMock, _appSettingsMock, _hostEnvironmentMock);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void Cookie_is_set_with_correct_parameters(string environment)
    {
        var guestToken = "token";
        var accessCookieName = "test-cookie";
        var httpContext = Fixture.Create<HttpContext>();
        _hostEnvironmentMock.EnvironmentName.Returns(environment);
        _appSettingsMock.Value.Jwt.AccessTokenCookieName = accessCookieName;


        _guestService.SetGuestCookie(guestToken, httpContext);

        var cookieOptions = new CookieOptions()
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = environment == "Production",
            SameSite = SameSiteMode.Strict,
        };

        httpContext.Response.Cookies.Received(1).Append(
            accessCookieName,
            guestToken,
            ArgEx.FluentAssert<CookieOptions>(x => x.Should().BeEquivalentTo(cookieOptions)));
    }
}
