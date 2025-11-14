using Bogus;
using AnarchyChess.Api.ArchivedGames.Entities;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class PlayerArchiveFaker : Faker<PlayerArchive>
{
    public PlayerArchiveFaker(GameColor color)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.UserName, f => f.Internet.UserName());
        RuleFor(x => x.UserId, f => (UserId)f.Random.Guid().ToString());
        RuleFor(x => x.Color, color);
        RuleFor(x => x.FinalTimeRemaining, f => f.Random.Double(0, 100000));
        RuleFor(x => x.NewRating, f => f.Random.Int(1200, 2500));
        RuleFor(x => x.RatingChange, f => f.Random.Int(-32, 32));
        RuleFor(x => x.CountryCode, f => f.Address.CountryCode());
    }
}
