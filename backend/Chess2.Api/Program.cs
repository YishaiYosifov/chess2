using Chess2.Api.Middlewares;
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

builder.Services.AddTransient<ExceptionHandlerMiddleware>();

# region Database
builder.Services.AddDbContextPool<Chess2DbContext>((serviceProvider, options) =>
    options.UseNpgsql(appSettings.Database.GetConnectionString())
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<IUserRepository, UserRepository>();
# endregion

# region Authentication
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.RequireHttpsMetadata = false;
        opts.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("")),
            ValidIssuer = appSettings.Jwt.Issuer,
            ValidAudience = appSettings.Jwt.Audience,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenProvider, TokenProvider>();
# endregion

# region Services
builder.Services.AddScoped<IUserService, UserService>();
# endregion

# region Validation
builder.Services.AddScoped<IValidator<UserIn>, UserValidator>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// for integration testing :)
public partial class Program { }