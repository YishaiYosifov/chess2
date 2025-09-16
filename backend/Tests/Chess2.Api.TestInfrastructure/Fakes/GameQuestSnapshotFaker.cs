using Bogus;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameQuestSnapshotFaker : RecordFaker<GameQuestSnapshot>
{
    public GameQuestSnapshotFaker(GameColor? playerColor = null)
    {
        StrictMode(true);
        RuleFor(x => x.GameToken, f => f.Random.Guid().ToString()[..16]);
        RuleFor(x => x.PlayerColor, f => playerColor ?? f.PickRandom<GameColor>());
        RuleFor(x => x.MoveHistory, f => new MoveFaker().Generate(5));
        RuleFor(x => x.ResultData, f => new GameResultDataFaker().Generate());
    }
}

public static class GameQuestSnapshotFakerExtensions
{
    public static Faker<GameQuestSnapshot> RuleForMoves(
        this Faker<GameQuestSnapshot> faker,
        IEnumerable<Move>? whiteMoves = null,
        IEnumerable<Move>? blackMoves = null,
        int? totalPlies = null
    )
    {
        totalPlies ??= Math.Max(whiteMoves?.Count() ?? 0, blackMoves?.Count() ?? 0) * 2;

        var whiteEnumerator = (whiteMoves ?? []).GetEnumerator();
        var blackEnumerator = (blackMoves ?? []).GetEnumerator();

        var whiteFaker = new MoveFaker(GameColor.White);
        var blackFaker = new MoveFaker(GameColor.Black);

        var history = new List<Move>();
        for (int i = 0; i < totalPlies; i++)
        {
            if (i % 2 == 0)
            {
                history.Add(
                    whiteEnumerator.MoveNext() ? whiteEnumerator.Current : whiteFaker.Generate()
                );
            }
            else
            {
                history.Add(
                    blackEnumerator.MoveNext() ? blackEnumerator.Current : blackFaker.Generate()
                );
            }
        }

        return faker.RuleFor(x => x.MoveHistory, history);
    }

    public static Faker<GameQuestSnapshot> RuleForWin(
        this Faker<GameQuestSnapshot> faker,
        GameColor playerColor
    ) =>
        faker
            .RuleFor(x => x.PlayerColor, playerColor)
            .RuleFor(
                x => x.ResultData,
                f => new GameResultDataFaker(
                    playerColor.Match(
                        whenWhite: GameResult.WhiteWin,
                        whenBlack: GameResult.BlackWin
                    )
                )
            );

    public static Faker<GameQuestSnapshot> RuleForLoss(
        this Faker<GameQuestSnapshot> faker,
        GameColor playerColor
    ) =>
        faker
            .RuleFor(x => x.PlayerColor, playerColor)
            .RuleFor(
                x => x.ResultData,
                f => new GameResultDataFaker(
                    playerColor.Match(
                        whenWhite: GameResult.BlackWin,
                        whenBlack: GameResult.WhiteWin
                    )
                )
            );
}
