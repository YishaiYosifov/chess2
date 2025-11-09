using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameStateFaker : RecordFaker<GameState>
{
    public GameStateFaker()
    {
        StrictMode(true);
        RuleFor(x => x.Revision, f => f.Random.Number(1, 100));
        RuleFor(x => x.GameSource, f => f.PickRandom<GameSource>());
        RuleFor(
            x => x.Pool,
            f => new PoolKey(
                PoolType: f.PickRandom<PoolType>(),
                TimeControl: new(
                    BaseSeconds: f.Random.Number(300, 600),
                    IncrementSeconds: f.Random.Number(3, 10)
                )
            )
        );
        RuleFor(x => x.WhitePlayer, f => new GamePlayerFaker(GameColor.White).Generate());
        RuleFor(x => x.BlackPlayer, f => new GamePlayerFaker(GameColor.Black).Generate());
        RuleFor(
            x => x.Clocks,
            f => new ClockSnapshot(
                WhiteClock: f.Random.Double(1000, 100000),
                BlackClock: f.Random.Double(1000, 100000),
                LastUpdated: f.Random.Double(1000000, 10000000)
            )
        );
        RuleFor(x => x.SideToMove, f => f.PickRandom<GameColor>());
        RuleFor(x => x.InitialFen, "10/10/10/10/10/10/10/10/10/10");
        RuleFor(x => x.MoveOptions, f => new MoveOptionsFaker().Generate());
        RuleFor(x => x.MoveHistory, f => new MoveSnapshotFaker().Generate(f.Random.Number(1, 6)));
        RuleFor(x => x.DrawState, new DrawState());
        RuleFor(x => x.ResultData, (GameResultData?)null);
    }
}
