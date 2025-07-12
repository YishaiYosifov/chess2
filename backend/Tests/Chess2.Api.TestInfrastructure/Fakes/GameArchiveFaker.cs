using Bogus;
using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameArchiveFaker : Faker<GameArchive>
{
    public GameArchiveFaker(
        PlayerArchive? whitePlayer = null,
        PlayerArchive? blackPlayer = null,
        int moveCount = 5
    )
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.GameToken, f => f.Random.Guid().ToString()[..16]);
        RuleFor(x => x.Result, f => f.PickRandom<GameResult>());
        RuleFor(x => x.ResultDescription, "some description");
        RuleFor(x => x.FinalFen, "10/10/10/10/10/10/10/10/10/10");
        RuleFor(x => x.Moves, new MoveArchiveFaker().Generate(moveCount));
        RuleFor(x => x.IsRated, f => f.Random.Bool());

        RuleFor(x => x.BaseSeconds, f => f.Random.Int(60, 6000));
        RuleFor(x => x.IncrementSeconds, f => f.Random.Int(1, 30));

        RuleFor(
            x => x.WhitePlayer,
            f => whitePlayer ?? new PlayerArchiveFaker(GameColor.White).Generate()
        );
        RuleFor(x => x.WhitePlayerId, (f, g) => g.WhitePlayer?.Id);

        RuleFor(
            x => x.BlackPlayer,
            f => blackPlayer ?? new PlayerArchiveFaker(GameColor.Black).Generate()
        );
        RuleFor(x => x.BlackPlayerId, (f, g) => g.WhitePlayer?.Id);

        RuleFor(g => g.CreatedAt, f => f.Date.PastOffset(1).UtcDateTime);
    }
}
