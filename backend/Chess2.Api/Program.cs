using Akka.Cluster.Hosting;
using Akka.Hosting;
using Akka.Remote.Hosting;
using Chess2.Api.Auth.Errors;
using Chess2.Api.Auth.Repositories;
using Chess2.Api.Auth.Services;
using Chess2.Api.Auth.Services.OAuthAuthenticators;
using Chess2.Api.Game.Services;
using Chess2.Api.Game.SignalR;
using Chess2.Api.GameLogic;
using Chess2.Api.GameLogic.PieceDefinitions;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.ActionFilters;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Infrastructure.OpenAPI;
using Chess2.Api.Matchmaking.Actors;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Matchmaking.SignalR;
using Chess2.Api.Shared.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.UserRating.Services;
using Chess2.Api.Users.DTOs;
using Chess2.Api.Users.Entities;
using Chess2.Api.Users.Services;
using Chess2.Api.Users.Validators;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag.Generation.Processors;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("chess2.log")
    .CreateLogger();
builder.Services.AddSerilog();

var appSettingsSection = builder.Configuration.GetSection(nameof(AppSettings));
builder.Services.Configure<AppSettings>(appSettingsSection);
var resolvedAppSettings =
    builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()
    ?? throw new InvalidOperationException("AppSettings missing");

builder
    .Services.AddControllers()
    .AddNewtonsoftJson()
    .AddMvcOptions(options =>
    {
        options.Conventions.Add(new UnauthorizedResponseConvention());
        options.Filters.Add<ReformatValidationProblemAttribute>();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(options =>
{
    options.SchemaSettings.SchemaNameGenerator = new OpenAPIDisplayNameSchemaNameGenerator();
    options.SchemaSettings.SchemaProcessors.Add(new MarkAsRequiredIfNonNullableSchemaProcessor());
});
builder.Services.AddSingleton<IDocumentProcessor, ErrorCodesDocumentProcessor>();
builder.Services.AddSingleton<IOperationProcessor, MethodNameOperationIdProcessor>();

const string AllowCorsOriginName = "AllowCorsOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        AllowCorsOriginName,
        policy =>
            policy
                .WithOrigins(resolvedAppSettings.CorsOrigins)
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

builder.Services.AddSignalR().AddStackExchangeRedis();

#region Database
builder.Services.AddDbContextPool<ApplicationDbContext>(
    (serviceProvider, options) =>
    {
        var runtimeAppSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        options.UseNpgsql(runtimeAppSettings.DatabaseConnString).UseSnakeCaseNamingConvention();
    }
);
builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var runtimeAppSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
    return ConnectionMultiplexer.Connect(runtimeAppSettings.RedisConnString);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
#endregion

#region Authentication
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthPolicies.RefreshAccess,
        policy => policy.RequireClaim("type", "refresh").AddAuthenticationSchemes("RefreshBearer")
    );

    options.AddPolicy(
        AuthPolicies.AuthedUser,
        policy =>
            policy
                .RequireAssertion(context =>
                {
                    var isAccess = context.User.HasClaim("type", "access");
                    var isAnonymous = context.User.HasClaim(ClaimTypes.Anonymous, "1");
                    return isAccess && !isAnonymous;
                })
                .AddAuthenticationSchemes("AccessBearer")
    );

    options.AddPolicy(
        AuthPolicies.AuthedSesssion,
        policy => policy.RequireClaim("type", "access").AddAuthenticationSchemes("AccessBearer")
    );

    options.DefaultPolicy = options.GetPolicy(AuthPolicies.AuthedUser)!;
});
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            options.DefaultChallengeScheme =
            options.DefaultForbidScheme =
            options.DefaultSignOutScheme =
            options.DefaultSignInScheme =
            options.DefaultScheme =
                JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(
        "AccessBearer",
        options => ConfigureJwtBearerCookie(options, resolvedAppSettings.Jwt.AccessTokenCookieName)
    )
    .AddJwtBearer(
        "RefreshBearer",
        options => ConfigureJwtBearerCookie(options, resolvedAppSettings.Jwt.RefreshTokenCookieName)
    );

void ConfigureJwtBearerCookie(JwtBearerOptions options, string cookieName)
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.TokenValidationParameters = new()
    {
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(resolvedAppSettings.Jwt.SecretKey)
        ),
        ValidIssuer = resolvedAppSettings.Jwt.Issuer,
        ValidAudience = resolvedAppSettings.Jwt.Audience,
        ClockSkew = TimeSpan.Zero,
    };

    options.Events = new()
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.TryGetValue(cookieName, out var token))
                ctx.Token = token;
            else if (ctx.Request.Headers.TryGetValue("Authorization", out var headerToken))
                ctx.Token = headerToken;

            return Task.CompletedTask;
        },

        // set a custom unauthorized response
        OnChallenge = ctx =>
        {
            ctx.HandleResponse();
            Error error = ctx.Request.Cookies.ContainsKey(cookieName)
                ? AuthErrors.TokenInvalid
                : AuthErrors.TokenMissing;
            return error
                .ToActionResult()
                .ExecuteResultAsync(new() { HttpContext = ctx.HttpContext });
        },
    };
}

builder
    .Services.AddOpenIddict()
    .AddClient(options =>
    {
        options.DisableTokenStorage();
        options.AllowAuthorizationCodeFlow();

        options
            .UseWebProviders()
            .AddGoogle(options =>
            {
                var clientId =
                    builder.Configuration["Authentication:Google:ClientId"]
                    ?? throw new KeyNotFoundException("Google OAuth Client Id");
                var clientSecret =
                    builder.Configuration["Authentication:Google:ClientSecret"]
                    ?? throw new KeyNotFoundException("Google OAuth Client Secret");

                options.SetClientId(clientId);
                options.SetClientSecret(clientSecret);
                options.AddScopes("email");
                options.SetRedirectUri("api/oauth/google/callback");
            })
            .AddDiscord(options =>
            {
                var clientId =
                    builder.Configuration["Authentication:Discord:ClientId"]
                    ?? throw new KeyNotFoundException("Discord OAuth Client Id");
                var clientSecret =
                    builder.Configuration["Authentication:Discord:ClientSecret"]
                    ?? throw new KeyNotFoundException("Discord OAuth Client Secret");

                options.SetClientId(clientId);
                options.SetClientSecret(clientSecret);
                options.SetRedirectUri("api/oauth/discord/callback");
            });

        options.UseAspNetCore().EnableRedirectionEndpointPassthrough();
        options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
    });

builder
    .Services.AddIdentityCore<AuthedUser>(options =>
    {
        options.Password = new()
        {
            RequireDigit = false,
            RequiredLength = 8,
            RequireLowercase = false,
            RequireUppercase = false,
            RequireNonAlphanumeric = false,
        };
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddScoped<IOAuthProviderNameNormalizer, OAuthProviderNameNormalizer>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<IAuthCookieSetter, AuthCookieSetter>();

builder.Services.AddScoped<IOAuthAuthenticator, GoogleOAuthAuthenticator>();
builder.Services.AddScoped<IOAuthAuthenticator, DiscordOAuthAuthenticator>();

builder.Services.AddScoped<IUsernameGenerator, UsernameGenerator>();
builder.Services.AddSingleton<IUsernameWordsProvider, UsernameWordsProvider>();
#endregion

#region Validation
ValidatorOptions.Global.PropertyNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;
builder.Services.AddScoped<IValidator<ProfileEditRequest>, ProfileEditValidator>();
#endregion

#region Akka
builder.Services.AddAkka(
    resolvedAppSettings.Akka.ActorSystemName,
    (akkaBuilder, serviceProvider) =>
    {
        var runtimeAppSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;

        akkaBuilder.ConfigureLoggers(logConfig =>
        {
            logConfig.ClearLoggers();
            logConfig.AddLoggerFactory();
        });
        akkaBuilder
            .WithRemoting(runtimeAppSettings.Akka.Hostname, runtimeAppSettings.Akka.Port)
            .WithClustering(
                new()
                {
                    SeedNodes = runtimeAppSettings.Akka.SeedNodes,
                    Roles = [ActorSystemConstants.BackendRole],
                }
            )
            .WithMatchmakingShard<RatedMatchmakingActor>(
                "rated-matchmaking",
                runtimeAppSettings.Akka.MatchmakingShardCount
            )
            .WithMatchmakingShard<CasualMatchmakingActor>(
                "casual-matchmaking",
                runtimeAppSettings.Akka.MatchmakingShardCount
            )
            .WithPlayerShard(runtimeAppSettings.Akka.PlayerShardCount)
            .WithGameShard(runtimeAppSettings.Akka.GameShardCount);
    }
);
#endregion

#region Game
builder.Services.AddTransient<IRatedMatchmakingPool, RatedMatchmakingPool>();
builder.Services.AddTransient<ICasualMatchmakingPool, CasualMatchmakingPool>();
builder.Services.AddScoped<IMatchmakingService, MatchmakingService>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddSingleton<IGameTokenGenerator, GameTokenGenerator>();
builder.Services.AddTransient<IGameCore, GameCore>();
builder.Services.AddTransient<IPlayerRoster, PlayerRoster>();

builder.Services.AddSingleton<IFenCalculator, FenCalculator>();
builder.Services.AddSingleton<IPieceToLetter, PieceToLetter>();
builder.Services.AddSingleton<ITimeControlTranslator, TimeControlTranslator>();
builder.Services.AddScoped<IRatingService, RatingService>();

builder.Services.AddSingleton<ILegalMoveCalculator, LegalMoveCalculator>();
builder.Services.AddSingleton<IMoveEncoder, MoveEncoder>();

builder.Services.AddSingleton<IPieceDefinition, KingDefinition>();
builder.Services.AddSingleton<IPieceDefinition, QueenDefinition>();
builder.Services.AddSingleton<IPieceDefinition, PawnDefinition>();
builder.Services.AddSingleton<IPieceDefinition, RookDefinition>();
builder.Services.AddSingleton<IPieceDefinition, BishopDefinition>();
builder.Services.AddSingleton<IPieceDefinition, HorseyDefinition>();
#endregion

builder.Services.AddSingleton<IRandomCodeGenerator, RandomCodeGenerator>();
builder.Services.AddSingleton<IIRandomProvider, RandomProvider>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(options =>
    {
        options.Path = "/openapi/v1.json";
    });
    app.MapScalarApiReference();
    app.ApplyMigrations();
}

app.UseCors(AllowCorsOriginName);

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

app.MapHub<MatchmakingHub>("/api/hub/matchmaking");
app.MapHub<GameHub>("/api/hub/game");

app.Run();

// expose the program for WebApplicationFactory
public partial class Program;
