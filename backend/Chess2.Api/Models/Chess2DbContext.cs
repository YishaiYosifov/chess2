using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Models;

public class Chess2DbContext : DbContext
{
    public Chess2DbContext(DbContextOptions<Chess2DbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Make comparisons case insensitive
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("NOCASE");
        foreach (var p in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(string)))
        {
            p.SetCollation("NOCASE");
        }

    }
}
