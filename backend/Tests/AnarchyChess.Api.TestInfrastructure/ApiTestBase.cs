using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.TestInfrastructure.Utils;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Sinks.XUnit.Injectable.Abstract;

namespace AnarchyChess.Api.TestInfrastructure;

public class ApiTestBase : IAsyncLifetime
{
    public AnarchyChessWebApplicationFactory Factory { get; }
    public IServiceScope Scope { get; }
    public ApiClient ApiClient { get; }

    public ApplicationDbContext DbContext { get; }
    public ITokenProvider TokenProvider { get; }
    public AppSettings AppSettings { get; }

    public AuthTestUtils AuthUtils { get; }

    public CancellationToken CT { get; } = TestContext.Current.CancellationToken;

    public ApiTestBase(AnarchyChessWebApplicationFactory factory)
    {
        Factory = factory;
        Factory.Server.PreserveExecutionContext = true;
        var outputSink = Factory.Services.GetRequiredService<IInjectableTestOutputSink>();
        outputSink.Inject(TestContext.Current.TestOutputHelper!);

        Scope = Factory.Services.CreateScope();
        ApiClient = Factory.CreateApiClient();

        DbContext = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        AppSettings = Scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        TokenProvider = Scope.ServiceProvider.GetRequiredService<ITokenProvider>();
        var refreshTokenService = Scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        AuthUtils = new(TokenProvider, refreshTokenService, AppSettings.Auth, DbContext);

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

    protected HubConnection GuestSignalR(string path, UserId? guestId = null)
    {
        var token = TokenProvider.GenerateGuestToken(guestId ?? UserId.Guest());
        var conn = SignalR(path, token);
        return conn;
    }

    protected HubConnection AuthedSignalR(string path, AuthedUser user)
    {
        var token = TokenProvider.GenerateAccessToken(user);
        var conn = SignalR(path, token.Value);
        return conn;
    }

    protected HubConnection SignalR(string path, string? accessToken = null)
    {
        var baseAddress =
            ApiClient.Client.BaseAddress
            ?? throw new InvalidOperationException("Base address is not set for ApiClient");
        var server = Factory.Server;

        var connection = new HubConnectionBuilder()
            .WithUrl(
                "http://localhost" + path,
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                    if (!string.IsNullOrEmpty(accessToken))
                        options.Headers.Add(
                            "Cookie",
                            $"{AppSettings.Auth.AccessTokenCookieName}={accessToken}"
                        );
                    options.Transports = HttpTransportType.LongPolling;
                }
            )
            .Build();

        return connection;
    }

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual async ValueTask DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
        Scope?.Dispose();
        DbContext?.Dispose();

        GC.SuppressFinalize(this);
    }
}
