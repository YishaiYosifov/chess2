using System.Security.Claims;
using AutoFixture;
using Chess2.Api.Models;
using Chess2.Api.Services;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        var loggerMock = Fixture.Create<ILogger<GuestService>>();
        _guestService = new(loggerMock, _tokenProviderMock, _appSettingsMock, _hostEnvironmentMock);
    }

    [Fact]
    public void Correct_token_is_returned()
    {
        var guestToken = "token";
        _tokenProviderMock.GenerateGuestToken(Arg.Any<string>()).Returns(guestToken);

        var result = _guestService.CreateGuestUser();

        result.Should().Be(guestToken);
    }

    [Fact]
    public void Guest_id_is_created_randomly()
    {
        _guestService.CreateGuestUser();
        _guestService.CreateGuestUser();

        _tokenProviderMock.Received(2).GenerateGuestToken(Arg.Any<string>());

        var calls = _tokenProviderMock.ReceivedCalls();
        var guestId1 = calls.ElementAt(0).GetArguments()[0];
        var guestId2 = calls.ElementAt(1).GetArguments()[0];
        guestId1.Should().NotBe(guestId2);
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

        httpContext
            .Response.Cookies.Received(1)
            .Append(
                accessCookieName,
                guestToken,
                ArgEx.FluentAssert<CookieOptions>(x => x.Should().BeEquivalentTo(cookieOptions))
            );
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("0", false)]
    [InlineData("1", true)]
    public void Guest_is_detected_correctly(string anonymousClaim, bool isGuest)
    {
        var principles = new ClaimsPrincipal();
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Anonymous, anonymousClaim)]);
        principles.AddIdentity(identity);

        var result = _guestService.IsGuest(principles);

        result.Should().Be(isGuest);
    }
}
