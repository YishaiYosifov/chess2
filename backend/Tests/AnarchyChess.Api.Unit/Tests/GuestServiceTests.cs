using System.Security.Claims;
using AnarchyChess.Api.Auth.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests;

public class GuestServiceTests : BaseUnitTest
{
    private readonly ITokenProvider _tokenProviderMock = Substitute.For<ITokenProvider>();
    private readonly GuestService _guestService;

    public GuestServiceTests()
    {
        _guestService = new(Substitute.For<ILogger<GuestService>>(), _tokenProviderMock);
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
    [InlineData("", false)]
    [InlineData("false", false)]
    [InlineData("true", true)]
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
