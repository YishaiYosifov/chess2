using Chess2.Api.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.TestInfrastructure;

public class ApiTestBase : IAsyncLifetime
{
    protected readonly IServiceScope Scope;
    protected readonly IChess2Api ApiClient;
    protected readonly Chess2DbContext DbContext;
    protected readonly Chess2WebApplicationFactory Factory;

    protected ApiTestBase(Chess2WebApplicationFactory factory)
    {
        Factory = factory;
        Scope = Factory.Services.CreateScope();

        ApiClient = Factory.CreateTypedClient();
        DbContext = Scope.ServiceProvider.GetRequiredService<Chess2DbContext>();

        // postgres can only store up to microsecond percision,
        // while c# DateTime also stores nanoseconds
        AssertionOptions.AssertEquivalencyUsing(options =>
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
