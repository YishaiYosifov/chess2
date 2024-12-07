using AutoFixture;
using Chess2.Api.Services;

namespace Chess2.Api.Unit.Tests;

public class GuestServiceTests : BaseUnitTest
{
    private readonly GuestService _passwordHasher;

    public GuestServiceTests()
    {
        _passwordHasher = Fixture.Create<GuestService>();
    }

    [Fact]
    public void Trest()
    {

    }
}
