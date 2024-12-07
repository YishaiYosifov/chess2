using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Chess2.Api.Models;
using Chess2.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

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
