using Chess2.Api.Auth.Services;
using Chess2.Api.Infrastructure;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Chess2.Api.TestInfrastructure;

public class ApiTestBase : IAsyncLifetime
{
    protected readonly Chess2WebApplicationFactory Factory;
    protected readonly IServiceScope Scope;
    protected readonly ApiClient ApiClient;

    protected readonly ApplicationDbContext DbContext;
    protected readonly ITokenProvider TokenProvider;
    protected readonly AppSettings AppSettings;

    protected readonly AuthTestUtils AuthUtils;

    protected ApiTestBase(Chess2WebApplicationFactory factory)
    {
        Factory = factory;
        Scope = Factory.Services.CreateScope();
        ApiClient = Factory.CreateApiClient();

        DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        AppSettings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        TokenProvider = Scope.ServiceProvider.GetRequiredService<ITokenProvider>();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        AuthUtils = new(TokenProvider, refreshTokenService, AppSettings.Jwt, DbContext);

        // postgres can only store up to microsecond percision,
        // while c# DateTime also stores nanoseconds
        AssertionConfiguration.Current.Equivalency.Modify(options =>
            options
                .Using<DateTime>(ctx =>
                    ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMicroseconds(1))
                )
                .WhenTypeIs<DateTime>()
        );
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
        Scope?.Dispose();
        DbContext?.Dispose();
    }
}
