using Bogus;
using Chess2.Api.ArchivedGames.Entities;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class MoveSideEffectArchiveFaker : Faker<MoveSideEffectArchive>
{
    public MoveSideEffectArchiveFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Id, 0);
        RuleFor(x => x.FromIdx, f => (byte)f.Random.Number(0, 99));
        RuleFor(x => x.ToIdx, f => (byte)f.Random.Number(0, 99));
    }
}
