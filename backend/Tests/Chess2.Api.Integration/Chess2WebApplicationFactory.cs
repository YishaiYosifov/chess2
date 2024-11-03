using Chess2.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;

namespace Chess2.Api.Integration;

public class Chess2WebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // remove the existing database context, use an in-memory one instead
            services.RemoveAll(typeof(DbContextOptions<Chess2DbContext>));
            services.AddDbContextPool<Chess2DbContext>(options =>
                options.UseInMemoryDatabase("TestDB"));
        });
    }

    /// <summary>
    /// Create a client that follows the chess2 api schema
    /// and is authenticated with the api
    /// </summary>
    public IChess2Api CreateTypedAuthedClient(TestClaimsProvider claimsProvider)
    {
        var httpClient = WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices((services) =>
            {
                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });
                services.AddScoped(_ => claimsProvider);
            });
        }).CreateClient();

        return RestService.For<IChess2Api>(httpClient);
    }

    /// <summary>
    /// Create an http client that follows the chess2 api schema
    /// </summary>
    public IChess2Api CreateTypedClient() =>
        RestService.For<IChess2Api>(CreateClient());
}
