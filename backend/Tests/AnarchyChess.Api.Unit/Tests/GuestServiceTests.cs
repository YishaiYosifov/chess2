using AutoFixture;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Security.Claims;

namespace AnarchyChess.Api.Unit.Tests;

public class GuestServiceTests : BaseUnitTest
{
    private readonly IWebHostEnvironment _hostEnvironmentMock =
        Substitute.For<IWebHostEnvironment>();
    private readonly ITokenProvider _tokenProviderMock = Substitute.For<ITokenProvider>();

    private readonly IOptions<AppSettings> _settings;
    private readonly GuestService _guestService;

    public GuestServiceTests()
    {
        _settings = Fixture.Create<IOptions<AppSettings>>();
        _guestService = new(
            Substitute.For<ILogger<GuestService>>(),
            _tokenProviderMock,
            _settings,
            _hostEnvironmentMock
        );
    }

    [Fact]
    public void CreateGuestUser_correctly_generates_the_guest_token()
    {
        var guestToken = "token";
        _tokenProviderMock.GenerateGuestToken(Arg.Any<string>()).Returns(guestToken);

        var result = _guestService.CreateGuestUser();

        result.Should().Be(guestToken);
    }

    [Fact]
    public void CreateGuestUser_chooses_a_random_id()
    {
        _guestService.CreateGuestUser();
        _guestService.CreateGuestUser();

        _tokenProviderMock.Received(2).GenerateGuestToken(Arg.Any<string>());

        var calls = _tokenProviderMock.ReceivedCalls();
        var guestId1 = calls.ElementAt(0).GetArguments()[0];
        var guestId2 = calls.ElementAt(1).GetArguments()[0];
        guestId1.Should().NotBe(guestId2);

        guestId1.ToString().Should().StartWith("guest:");
        guestId2?.ToString().Should().StartWith("guest:");

        guestId1.ToString()?.Length.Should().BeGreaterThan(36);
        guestId2?.ToString()?.Length.Should().BeGreaterThan(36);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void SetGuestCookie_sets_the_cookie_with_the_correct_parameters(string environment)
    {
        var guestToken = "token";
        var accessCookieName = "test-cookie";
        var httpContext = Fixture.Create<HttpContext>();
        _hostEnvironmentMock.EnvironmentName.Returns(environment);
        _settings.Value.Jwt.AccessTokenCookieName = accessCookieName;

        _guestService.SetGuestCookie(guestToken, httpContext);

        var cookieOptions = new CookieOptions()
        {
            HttpOnly = true,
            IsEssential = true,
            Secure = true,
            SameSite = SameSiteMode.None,
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
    public void IsGuest_correctly_determines_if_a_user_is_a_guest_or_not(
        string anonymousClaim,
        bool isGuest
    )
    {
        var principles = new ClaimsPrincipal();
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Anonymous, anonymousClaim)]);
        principles.AddIdentity(identity);

        var result = _guestService.IsGuest(principles);

        result.Should().Be(isGuest);
    }

    [Fact]
    public void IsGuest_doesnt_detect_null_claims_as_guests()
    {
        var result = _guestService.IsGuest(null);
        result.Should().BeFalse();
    }
}
