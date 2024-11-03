using Chess2.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
