using Chess2.Api.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AuthedUser, IdentityRole<int>, int>(options)
{
    public required DbSet<RefreshToken> RefreshTokens { get; set; }

    public required DbSet<Rating> Ratings { get; set; }
}
