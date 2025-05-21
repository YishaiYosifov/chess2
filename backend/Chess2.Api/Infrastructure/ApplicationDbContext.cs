using Chess2.Api.Auth.Entities;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AuthedUser, IdentityRole<int>, int>(options)
{
    public required DbSet<RefreshToken> RefreshTokens { get; set; }

    public required DbSet<Rating> Ratings { get; set; }
}
