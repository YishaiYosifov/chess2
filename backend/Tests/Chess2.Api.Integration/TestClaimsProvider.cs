using Chess2.Api.Models.Entities;
using System.Security.Claims;

namespace Chess2.Api.Integration;

public class TestClaimsProvider
{
    public readonly IReadOnlyCollection<Claim> Claims;

    public TestClaimsProvider(IEnumerable<Claim> claims)
    {
        Claims = claims.ToList().AsReadOnly();
    }

    public TestClaimsProvider()
    {
        Claims = [];
    }

    public static TestClaimsProvider WithUser(User user)
    {
        return new TestClaimsProvider([new Claim(
            ClaimTypes.NameIdentifier,
            user.UserId.ToString())]);
    }
}
