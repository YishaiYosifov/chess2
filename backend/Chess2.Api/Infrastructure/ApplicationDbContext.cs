using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.Auth.Entities;
using Chess2.Api.LiveGame.Entities;
using Chess2.Api.Preferences.Entities;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Social.Entities;
using Chess2.Api.UserRating.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chess2.Api.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AuthedUser>(options)
{
    public required DbSet<RefreshToken> RefreshTokens { get; set; }

    public required DbSet<UserPreferences> UserPreferences { get; set; }

    public required DbSet<BlockedUser> BlockedUsers { get; set; }
    public required DbSet<StarredUser> StarredUsers { get; set; }

    public required DbSet<CurrentRating> CurrentRatings { get; set; }
    public required DbSet<RatingArchive> RatingArchives { get; set; }

    public required DbSet<ChatMessage> MessagesLogs { get; set; }

    public required DbSet<GameArchive> GameArchives { get; set; }
    public required DbSet<PlayerArchive> PlayerArchives { get; set; }
    public required DbSet<MoveArchive> MoveArchives { get; set; }
    public required DbSet<MoveSideEffectArchive> MoveSideEffectArchives { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<StarredUser>().Navigation(x => x.Starred).AutoInclude();
        builder.Entity<BlockedUser>().Navigation(x => x.Blocked).AutoInclude();

        base.OnModelCreating(builder);
    }
}
