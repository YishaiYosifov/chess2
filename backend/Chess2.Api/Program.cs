using Chess2.Api.Middlewares;
using Chess2.Api.Models;
using Chess2.Api.Repositories;
using Chess2.Api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("chess2.log")
    .CreateLogger();
builder.Services.AddSerilog();

var appConfigSection = builder.Configuration.GetSection(nameof(AppConfig));
builder.Services.Configure<AppConfig>(appConfigSection);
var appConfig = appConfigSection.Get<AppConfig>()
    ?? throw new InvalidOperationException("App config not provided in appsettings");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<ExceptionHandlerMiddleware>();

builder.Services.AddDbContextPool<Chess2DbContext>((serviceProvider, options) =>
    options.UseNpgsql(appConfig.Database.GetConnectionString())
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddSingleton<TokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// for integration testing :)
public partial class Program { }