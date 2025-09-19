using Bogus;
using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.TestData;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveArchiveFaker : Faker<MoveArchive>
{
    public MoveArchiveFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.MoveNumber, f => f.IndexFaker);
        RuleFor(x => x.San, f => f.PickRandom(MoveData.SanMoves));
        RuleFor(x => x.TimeLeft, f => f.Random.Double(1000, 10000));
        RuleFor(x => x.FromIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.ToIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.Captures, MoveData.RandomIdxs);
        RuleFor(x => x.Triggers, MoveData.RandomIdxs);
        RuleFor(x => x.Intermediates, MoveData.RandomIdxs);
        RuleFor(
            x => x.SideEffects,
            f => new MoveSideEffectArchiveFaker().Generate(f.Random.Number(1, 5))
        );
        RuleFor(x => x.PromotesTo, f => f.PickRandom<PieceType>());
    }
}
