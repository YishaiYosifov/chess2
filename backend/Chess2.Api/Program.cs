using System.Text;
using Chess2.Api.Extensions;
using Chess2.Api.Infrastructure;
using Chess2.Api.Infrastructure.ActionFilters;
using Chess2.Api.Models;
using Chess2.Api.Models.DTOs;
using Chess2.Api.Models.Entities;
using Chess2.Api.Repositories;
using Chess2.Api.Services;
using Chess2.Api.Services.Matchmaking;
using Chess2.Api.SignalR;
using Chess2.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
        options.Filters.Add<ReformatValidationProblemAttribute>();
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
builder.Services.AddDbContextPool<ApplicationDbContext>(
    (serviceProvider, options) =>
        options.UseNpgsql(appSettings.DatabaseConnString).UseSnakeCaseNamingConvention()
);
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(appSettings.RedisConnString)
);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IMatchmakingRepository, MatchmakingRepository>();
#endregion

#region Authentication
builder.Services.AddAuthorization();
builder
    .Services.AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme =
            opts.DefaultChallengeScheme =
            opts.DefaultForbidScheme =
            opts.DefaultSignOutScheme =
            opts.DefaultSignInScheme =
            opts.DefaultScheme =
                JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new()
        {
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(appSettings.Jwt.SecretKey)
            ),
            ValidIssuer = appSettings.Jwt.Issuer,
            ValidAudience = appSettings.Jwt.Audience,
        };
    });

builder
    .Services.AddIdentityCore<AuthedUser>(opts =>
        opts.Password = new()
        {
            RequireDigit = false,
            RequiredLength = 8,
            RequireLowercase = false,
            RequireUppercase = false,
            RequireNonAlphanumeric = false,
        }
    )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
#endregion

#region Validation
ValidatorOptions.Global.PropertyNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;
builder.Services.AddScoped<IValidator<ProfileEdit>, ProfileEditValidator>();
builder.Services.AddScoped<IValidator<SignupRequest>, SignupValidator>();
#endregion

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IMatchmakingService, MatchmakingService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
