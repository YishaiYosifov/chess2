using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GamePlayerFaker : RecordFaker<GamePlayer>
{
    public GamePlayerFaker(GameColor color)
    {
        StrictMode(true);
        RuleFor(p => p.UserId, f => f.Random.Guid().ToString());
        RuleFor(p => p.Color, color);
        RuleFor(p => p.UserName, f => f.Person.FullName);
        RuleFor(p => p.CountryCode, f => f.Address.CountryCode());
        RuleFor(p => p.Rating, f => f.Random.Int(1000, 3000));
    }
}
