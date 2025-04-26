using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public required DbSet<AuthedUser> Users { get; set; }
    public required DbSet<Rating> Ratings { get; set; }
}
