using Chess2.Api;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Validators;
using Chess2.Api.Repositories;
using Chess2.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
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
var appSettings = appSettingsSection.Get<AppSettings>()
    ?? throw new InvalidOperationException("App settings not provided in appsettings");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Database
builder.Services.AddDbContextPool<Chess2DbContext>((serviceProvider, options) =>
    options.UseNpgsql(appSettings.Database.GetConnectionString())
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<IUserRepository, UserRepository>();
#endregion

#region Authentication
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RefreshToken", policy =>
            policy.RequireClaim("type", "refresh")
            .AddAuthenticationSchemes("RefreshBearer"));
    options.AddPolicy("AccessToken", policy =>
            policy.RequireClaim("type", "access")
            .AddAuthenticationSchemes("AccessBearer"));
    options.DefaultPolicy = options.GetPolicy("AccessToken")!;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("AccessBearer", options =>
        ConfigureJwtBearerCookie(options, appSettings.Jwt.AccessTokenCookieName))
    .AddJwtBearer("RefreshBearer", options =>
        ConfigureJwtBearerCookie(options, appSettings.Jwt.RefreshTokenCookieName));

void ConfigureJwtBearerCookie(JwtBearerOptions options, string cookieName)
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new()
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.SecretKey)),
        ValidIssuer = appSettings.Jwt.Issuer,
        ValidAudience = appSettings.Jwt.Audience,
        ClockSkew = TimeSpan.Zero,
    };

    options.Events = new JwtBearerEvents()
    {
        OnMessageReceived = ctx =>
        {
            ctx.Request.Cookies.TryGetValue(cookieName, out var token);
            if (!string.IsNullOrEmpty(token)) ctx.Token = token;
            return Task.CompletedTask;
        }
    };
}

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
#endregion

#region Validation
builder.Services.AddScoped<IValidator<UserIn>, UserValidator>();
#endregion

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.Run();

// expose the program for WebApplicationFactory
public partial class Program;
