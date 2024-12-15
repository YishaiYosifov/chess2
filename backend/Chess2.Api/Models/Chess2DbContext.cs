using Chess2.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Models;

public class Chess2DbContext(DbContextOptions<Chess2DbContext> options) : DbContext(options)
{
    public required DbSet<AuthedUser> Users { get; set; }
    public required DbSet<Rating> Ratings { get; set; }
}
