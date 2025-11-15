using AnarchyChess.Api.ArchivedGames.Entities;
using AnarchyChess.Api.Auth.Entities;
using AnarchyChess.Api.Donations.Entities;
using AnarchyChess.Api.Game.Entities;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Preferences.Entities;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Quests.Entities;
using AnarchyChess.Api.Social.Entities;
using AnarchyChess.Api.Streaks.Entities;
using AnarchyChess.Api.UserRating.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Infrastructure;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AuthedUser, IdentityRole<UserId>, UserId>(options)
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
    public required DbSet<PieceSpawnArchive> PieceSpawnArchives { get; set; }

    public required DbSet<UserQuestPoints> QuestPoints { get; set; }

    public required DbSet<UserWinStreak> WinStreaks { get; set; }

    public required DbSet<Donation> Donations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<StarredUser>().Navigation(x => x.Starred).AutoInclude();
        builder.Entity<BlockedUser>().Navigation(x => x.Blocked).AutoInclude();
        builder.Entity<UserQuestPoints>().Navigation(x => x.User).AutoInclude();
        builder.Entity<UserWinStreak>().Navigation(x => x.User).AutoInclude();

        base.OnModelCreating(builder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<UserId>()
            .HaveConversion<StructStringValueConverter<UserId>>();
        configurationBuilder
            .Properties<GameToken>()
            .HaveConversion<StructStringValueConverter<GameToken>>();
        base.ConfigureConventions(configurationBuilder);
    }
}
