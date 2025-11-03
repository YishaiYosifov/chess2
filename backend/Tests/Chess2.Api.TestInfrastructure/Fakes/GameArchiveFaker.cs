using Bogus;
using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.TestInfrastructure.Fakes;

public class GameArchiveFaker : Faker<GameArchive>
{
    public GameArchiveFaker()
        : this(whitePlayer: null, blackPlayer: null) { }

    public GameArchiveFaker(int moveCount)
        : this(whitePlayer: null, blackPlayer: null, moveCount) { }

    public GameArchiveFaker(
        PlayerArchive? whitePlayer = null,
        PlayerArchive? blackPlayer = null,
        int moveCount = 5
    )
    {
        StrictMode(true);
        RuleFor(x => x.GameToken, f => (GameToken)f.Random.Guid().ToString()[..16]);
        RuleFor(x => x.Result, f => f.PickRandom<GameResult>());
        RuleFor(x => x.ResultDescription, "some description");
        RuleFor(x => x.InitialFen, "10/10/10/10/10/10/10/10/10/10");
        RuleFor(x => x.Moves, new MoveArchiveFaker().Generate(moveCount));
        RuleFor(x => x.PoolType, f => f.PickRandom<PoolType>());

        RuleFor(x => x.BaseSeconds, f => f.Random.Int(60, 6000));
        RuleFor(x => x.IncrementSeconds, f => f.Random.Int(1, 30));

        RuleFor(
            x => x.WhitePlayer,
            f => whitePlayer ?? new PlayerArchiveFaker(GameColor.White).Generate()
        );
        RuleFor(x => x.WhitePlayerId, (f, g) => g.WhitePlayer.Id);

        RuleFor(
            x => x.BlackPlayer,
            f => blackPlayer ?? new PlayerArchiveFaker(GameColor.Black).Generate()
        );
        RuleFor(x => x.BlackPlayerId, (f, g) => g.WhitePlayer.Id);

        RuleFor(
            g => g.CreatedAt,
            f => f.Date.PastOffset(yearsToGoBack: 1).UtcDateTime + TimeSpan.FromDays(f.IndexFaker)
        );
    }

    public GameArchiveFaker(
        UserId? whiteUserId = null,
        UserId? blackUserId = null,
        int moveCount = 5
    )
        : this(
            whiteUserId is not null
                ? new PlayerArchiveFaker(GameColor.White)
                    .RuleFor(x => x.UserId, whiteUserId)
                    .Generate()
                : null,
            blackUserId is not null
                ? new PlayerArchiveFaker(GameColor.Black)
                    .RuleFor(x => x.UserId, blackUserId)
                    .Generate()
                : null,
            moveCount
        ) { }
}
