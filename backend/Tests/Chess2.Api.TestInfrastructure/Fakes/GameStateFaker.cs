using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure.TestData;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameStateFaker : RecordFaker<GameState>
{
    public GameStateFaker()
    {
        StrictMode(true);
        RuleFor(
            x => x.TimeControl,
            f => new TimeControlSettings(
                BaseSeconds: f.Random.Number(60, 900),
                IncrementSeconds: f.Random.Number(0, 30)
            )
        );
        RuleFor(x => x.IsRated, f => f.Random.Bool());
        RuleFor(x => x.WhitePlayer, f => new GamePlayerFaker(GameColor.White).Generate());
        RuleFor(x => x.BlackPlayer, f => new GamePlayerFaker(GameColor.Black).Generate());
        RuleFor(
            x => x.Clocks,
            f => new ClockDto(
                WhiteClock: f.Random.Double(1000, 100000),
                BlackClock: f.Random.Double(1000, 100000),
                LastUpdated: f.Random.Double(1000000, 10000000)
            )
        );
        RuleFor(x => x.SideToMove, f => f.PickRandom<GameColor>());
        RuleFor(x => x.Fen, "10/10/10/10/10/10/10/10/10/10");
        RuleFor(
            x => x.LegalMoves,
            f =>
            {
                var count = f.Random.Number(1, MoveData.EncodedMoves.Length);
                return f.Random.Shuffle(MoveData.EncodedMoves).Take(count);
            }
        );
        RuleFor(x => x.MoveHistory, f => new MoveSnapshotFaker().Generate(f.Random.Number(1, 6)));
    }
}
