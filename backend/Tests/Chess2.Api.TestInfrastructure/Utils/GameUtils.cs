using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.UserRating.Entities;
using Chess2.Api.Users.Entities;

namespace Chess2.Api.TestInfrastructure.Utils;

public record StartGameResult(
    AuthedUser User1,
    CurrentRating User1Rating,
    AuthedUser User2,
    CurrentRating User2Rating,
    string GameToken
);

public static class GameUtils
{
    public static async Task<StartGameResult> CreateRatedGameAsync(
        ApplicationDbContext dbContext,
        ILiveGameService gameService
    )
    {
        var timeControl = new TimeControlSettings(30, 0);
        var user1 = await FakerUtils.StoreFakerAsync(dbContext, new AuthedUserFaker());
        var user2 = await FakerUtils.StoreFakerAsync(dbContext, new AuthedUserFaker());

        var user1Rating = await FakerUtils.StoreFakerAsync(
            dbContext,
            new CurrentRatingFaker(user1, 1200).RuleFor(x => x.TimeControl, TimeControl.Bullet)
        );
        var user2Rating = await FakerUtils.StoreFakerAsync(
            dbContext,
            new CurrentRatingFaker(user2, 1300).RuleFor(x => x.TimeControl, TimeControl.Bullet)
        );

        var gameToken = await gameService.StartGameAsync(
            user1.Id,
            user2.Id,
            timeControl,
            isRated: true
        );

        return new(user1, user1Rating, user2, user2Rating, gameToken);
    }
}
