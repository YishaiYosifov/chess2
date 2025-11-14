using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.UserRating.Entities;

namespace AnarchyChess.Api.TestInfrastructure.Utils;

public record StartGameResult(
    AuthedUser User1,
    CurrentRating User1Rating,
    AuthedUser User2,
    CurrentRating User2Rating,
    GameToken GameToken,
    PoolKey Pool
);

public static class GameUtils
{
    public static async Task<StartGameResult> CreateRatedGameAsync(
        ApplicationDbContext dbContext,
        IGameStarter gameStarter
    )
    {
        var user1 = new AuthedUserFaker().Generate();
        var user1Rating = new CurrentRatingFaker(user1, 1200)
            .RuleFor(x => x.TimeControl, TimeControl.Bullet)
            .Generate();

        var user2 = new AuthedUserFaker().Generate();
        var user2Rating = new CurrentRatingFaker(user2, 1300)
            .RuleFor(x => x.TimeControl, TimeControl.Bullet)
            .Generate();

        await dbContext.AddRangeAsync(user1, user1Rating, user2, user2Rating);
        await dbContext.SaveChangesAsync();

        TimeControlSettings timeControl = new(30, 0);
        PoolKey pool = new(PoolType.Rated, timeControl);
        var gameToken = await gameStarter.StartGameAsync(user1.Id, user2.Id, pool);

        return new(user1, user1Rating, user2, user2Rating, gameToken, pool);
    }
}
