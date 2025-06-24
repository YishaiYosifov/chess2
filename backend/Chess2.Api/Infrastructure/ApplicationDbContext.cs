using Chess2.Api.Auth.Entities;
using Chess2.Api.Game.Entities;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AuthedUser>(options)
{
    public required DbSet<RefreshToken> RefreshTokens { get; set; }

    public required DbSet<Rating> Ratings { get; set; }

    public required DbSet<GameArchive> GameArchives { get; set; }
    public required DbSet<PlayerArchive> PlayerArchives { get; set; }
    public required DbSet<MoveArchive> MoveArchives { get; set; }
}
