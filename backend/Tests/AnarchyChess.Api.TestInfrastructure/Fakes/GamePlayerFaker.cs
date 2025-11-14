using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Entities;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class GamePlayerFaker : RecordFaker<GamePlayer>
{
    public GamePlayerFaker(GameColor color, AuthedUser? user = null)
    {
        StrictMode(true);
        RuleFor(x => x.UserId, f => user?.Id ?? f.Random.Guid().ToString());
        RuleFor(x => x.Color, color);
        RuleFor(x => x.UserName, f => user?.UserName ?? f.Person.FullName);
        RuleFor(x => x.CountryCode, f => f.Address.CountryCode());
        RuleFor(x => x.Rating, f => f.Random.Int(1000, 3000));
    }
}
