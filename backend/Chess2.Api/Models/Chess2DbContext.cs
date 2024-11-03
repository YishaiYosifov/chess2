using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Models;

public class Chess2DbContext(DbContextOptions<DbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}
