using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Integration.Tests;

public class AuthTests(Chess2WebApplicationFactory factory) : IClassFixture<Chess2WebApplicationFactory>
{
    private readonly Chess2WebApplicationFactory _factory = factory;
    private readonly IChess2Api _apiClient = RestService.For<IChess2Api>(factory.CreateClient());

    [Fact]
    public async Task Test()
    {
        await _apiClient.Register(new() {
            Username = "asd",
            Password = "password",
        });
    }
}
