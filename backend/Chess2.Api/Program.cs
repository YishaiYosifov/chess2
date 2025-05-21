using System.ComponentModel;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Chess2.Api.Auth.Errors;
using Chess2.Api.Auth.Repositories;
using Chess2.Api.Auth.Services;
using Chess2.Api.Auth.Services.OAuthAuthenticators;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.ActionFilters;
using Chess2.Api.Infrastructure.Extensions;
using Chess2.Api.Matchmaking.Repositories;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Matchmaking.SignalR;
using Chess2.Api.Services;
using Chess2.Api.Shared.DTOs;
using Chess2.Api.Shared.Services;
using Chess2.Api.UserRating.Repositories;
using Chess2.Api.Users.DTOs;
using Chess2.Api.Users.Entities;
using Chess2.Api.Users.Services;
using Chess2.Api.Users.Validators;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;

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
var appSettings =
    appSettingsSection.Get<AppSettings>()
    ?? throw new InvalidOperationException("App settings not provided in appsettings");

builder
    .Services.AddControllers()
    .AddNewtonsoftJson()
    .AddMvcOptions(options =>
    {
        options.Conventions.Add(new UnauthorizedResponseConvention());
        options.Filters.Add<ReformatValidationProblemAttribute>();
    });
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<OpenAPIErrorCodesSchemaTransformer>();
    options.CreateSchemaReferenceId = typeInfo =>
    {
        var type = typeInfo.Type;
        var attribute = type.GetCustomAttribute<DisplayNameAttribute>();
        if (attribute is null)
            return OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);

        return attribute.DisplayName;
    };
});

const string AllowCorsOriginName = "AllowCorsOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        AllowCorsOriginName,
        policy =>
            policy
                .WithOrigins(appSettings.CorsOrigins)
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

builder.Services.AddSignalR();

#region Database
builder.Services.AddDbContextPool<ApplicationDbContext>(
    (serviceProvider, options) =>
        options.UseNpgsql(appSettings.DatabaseConnString).UseSnakeCaseNamingConvention()
);
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(appSettings.RedisConnString)
);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IMatchmakingRepository, MatchmakingRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
#endregion

#region Authentication
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "RefreshToken",
        policy => policy.RequireClaim("type", "refresh").AddAuthenticationSchemes("RefreshBearer")
    );

    options.AddPolicy(
        "AuthedAccess",
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
        "GuestAccess",
        policy => policy.RequireClaim("type", "access").AddAuthenticationSchemes("AccessBearer")
    );

    options.DefaultPolicy = options.GetPolicy("AuthedAccess")!;
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
        options => ConfigureJwtBearerCookie(options, appSettings.Jwt.AccessTokenCookieName)
    )
    .AddJwtBearer(
        "RefreshBearer",
        options => ConfigureJwtBearerCookie(options, appSettings.Jwt.RefreshTokenCookieName)
    );

void ConfigureJwtBearerCookie(JwtBearerOptions options, string cookieName)
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.TokenValidationParameters = new()
    {
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(appSettings.Jwt.SecretKey)
        ),
        ValidIssuer = appSettings.Jwt.Issuer,
        ValidAudience = appSettings.Jwt.Audience,
        ClockSkew = TimeSpan.Zero,
    };

    options.Events = new()
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.TryGetValue(cookieName, out var token))
                ctx.Token = token;
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
#endregion

#region Validation
ValidatorOptions.Global.PropertyNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;
builder.Services.AddScoped<IValidator<ProfileEditRequest>, ProfileEditValidator>();
#endregion

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<IIRandomProvider, RandomProvider>();
builder.Services.AddSingleton<ITimeControlTranslator, TimeControlTranslator>();
builder.Services.AddScoped<IMatchmakingService, MatchmakingService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IUsernameGenerator, UsernameGenerator>();
builder.Services.AddSingleton<IUsernameWordsProvider, UsernameWordsProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.ApplyMigrations();
}

app.UseCors(AllowCorsOriginName);

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

app.MapHub<MatchmakingHub>("/api/ws/matchmaking");

app.Run();

// expose the program for WebApplicationFactory
public partial class Program;
