using Chess2Backend.Models;
using Chess2Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var appConfigSection = builder.Configuration.GetSection(nameof(AppConfig));
builder.Services.Configure<AppConfig>(appConfigSection);
var appConfig = appConfigSection.Get<AppConfig>()
    ?? throw new InvalidOperationException("App config not provided in appsettings");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextPool<Chess2DbContext>((serviceProvider, options) =>
    options.UseNpgsql(appConfig.Database.GetConnectionString())
    .UseInternalServiceProvider(serviceProvider));

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
