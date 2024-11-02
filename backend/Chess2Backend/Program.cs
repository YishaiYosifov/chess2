using Chess2Backend.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<DatabaseConfig>(
    builder.Configuration.GetSection(nameof(DatabaseConfig)));

var dbConfig = builder.Configuration.GetSection(nameof(DatabaseConfig)).Get<DatabaseConfig>()
    ?? throw new ArgumentException("Database settings not provided in appsettings");

builder.Services.AddDbContext<Chess2DbContext>(options => options.UseNpgsql(dbConfig.GetConnectionString()));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
