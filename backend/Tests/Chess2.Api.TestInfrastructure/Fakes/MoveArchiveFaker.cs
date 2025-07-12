using Bogus;
using Chess2.Api.Game.Entities;
using Chess2.Api.TestInfrastructure.TestData;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveArchiveFaker : Faker<MoveArchive>
{
    public MoveArchiveFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.MoveNumber, f => f.IndexFaker);
        RuleFor(x => x.EncodedMove, f => f.PickRandom(MoveData.EncodedMoves));
        RuleFor(x => x.San, f => f.PickRandom(MoveData.SanMoves));
        RuleFor(x => x.TimeLeft, f => f.Random.Double(1000, 10000));
    }
}
