using Bogus;
using Chess2.Api.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveArchiveFaker : Faker<MoveArchive>
{
    private readonly string[] _encodedMovePool = [
        "e4e5",
        "f6i2",
        "e4g7b3!a1",
        "h3b2",
        "d2d4",
        "gf3e5",
        "e1g1h1f1",
        "e1b1a1c1"
    ];

    public MoveArchiveFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.MoveNumber, f => f.IndexFaker);
        RuleFor(x => x.EncodedMove, f => f.PickRandom(_encodedMovePool));
    }
}
