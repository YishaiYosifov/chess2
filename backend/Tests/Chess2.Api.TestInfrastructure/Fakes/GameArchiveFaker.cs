using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameArchiveFaker : Faker<GameArchive>
{
    public GameArchiveFaker(PlayerArchive whitePlayer, PlayerArchive blackPlayer, int moveCount = 5)
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.GameToken, f => f.Random.Guid().ToString()[..16]);
        RuleFor(x => x.Result, f => f.PickRandom<GameResult>());
        RuleFor(x => x.FinalFen, "10/10/10/10/10/10/10/10/10/10");
        RuleFor(x => x.Moves, new MoveArchiveFaker().Generate(moveCount));
        RuleFor(x => x.WhitePlayerId, whitePlayer.Id);
        RuleFor(x => x.WhitePlayer, whitePlayer);
        RuleFor(x => x.BlackPlayerId, blackPlayer.Id);
        RuleFor(x => x.BlackPlayer, blackPlayer);
        RuleFor(g => g.CreatedAt, f => f.Date.PastOffset(1).UtcDateTime);
    }
}
