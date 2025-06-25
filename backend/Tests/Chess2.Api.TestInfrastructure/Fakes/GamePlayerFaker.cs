using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GamePlayerFaker : RecordFaker<GamePlayer>
{
    public GamePlayerFaker(GameColor color)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => f.Random.Guid().ToString());
        RuleFor(x => x.Color, color);
        RuleFor(x => x.UserName, f => f.Person.FullName);
        RuleFor(x => x.CountryCode, f => f.Address.CountryCode());
        RuleFor(x => x.Rating, f => f.Random.Int(1000, 3000));
    }
}
