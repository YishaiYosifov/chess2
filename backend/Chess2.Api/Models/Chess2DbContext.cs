using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Models;

public class Chess2DbContext : DbContext
{
    public Chess2DbContext(DbContextOptions<Chess2DbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }
}
