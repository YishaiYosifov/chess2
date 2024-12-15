using System.Security.Claims;
using System.Text;
using Chess2.Api.Errors;
using Chess2.Api.Extensions;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.ActionFilters;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Repositories;
using Chess2.Api.Services;
using Chess2.Api.SignalR;
using Chess2.Api.Validators;
using ErrorOr;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        options.Filters.Add(typeof(ReformatValidationProblemAttribute));
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddDbContextPool<Chess2DbContext>(
    (serviceProvider, options) =>
        options.UseNpgsql(appSettings.DatabaseConnString).UseSnakeCaseNamingConvention()
);
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(appSettings.RedisConnString)
);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
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
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
    options.RequireHttpsMetadata = false;
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
            ctx.Request.Cookies.TryGetValue(cookieName, out var token);
            if (!string.IsNullOrEmpty(token))
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
                .ToProblemDetails()
                .ExecuteResultAsync(new() { HttpContext = ctx.HttpContext });
        },
    };
}

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddSingleton<IGuestService, GuestService>();
builder.Services.AddScoped<IAuthService, AuthService>();
#endregion

#region Validation
ValidatorOptions.Global.PropertyNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;
builder.Services.AddScoped<IValidator<UserIn>, UserValidator>();
builder.Services.AddScoped<IValidator<ProfileEdit>, ProfileEditValidator>();
#endregion

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<IMatchmakingService, MatchmakingService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(AllowCorsOriginName);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.MapHub<MatchmakingHub>("/api/ws/matchmaking");

app.Run();

// expose the program for WebApplicationFactory
public partial class Program;
