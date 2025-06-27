using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.UserRating.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.GameArchiveTests;

public class GameArchiveServiceTests : BaseIntegrationTest
{
    private readonly IGameArchiveService _gameArchiveService;

    public GameArchiveServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameArchiveService = Scope.ServiceProvider.GetRequiredService<IGameArchiveService>();
    }

    [Fact]
    public async Task CreateArchiveAsync_creates_and_saves_the_game_archive_correctly()
    {
        var gameToken = Guid.NewGuid().ToString("N")[..16];
        var gameState = CreateTestGameState();
        var expectedResult = GameResult.WhiteWin;
        var ratingDelta = new RatingDelta(WhiteDelta: 100, BlackDelta: -150);

        var result = await _gameArchiveService.CreateArchiveAsync(
            gameToken,
            gameState,
            expectedResult,
            ratingDelta,
            CT
        );

        await DbContext.SaveChangesAsync(CT);

        var savedArchive = await GetSavedArchiveAsync(gameToken);
        // Verify service returned the same object that was saved
        savedArchive.Should().BeEquivalentTo(result);

        var expectedArchive = new GameArchive
        {
            GameToken = gameToken,
            Result = expectedResult,
            FinalFen = gameState.Fen,
            WhitePlayer = CreateExpectedPlayerArchive(
                gameState.WhitePlayer,
                ratingDelta.WhiteDelta
            ),
            BlackPlayer = CreateExpectedPlayerArchive(
                gameState.BlackPlayer,
                ratingDelta.BlackDelta
            ),
            Moves = CreateExpectedMoveArchives(gameState.MoveHistory),
        };

        // Verify archive properties match expected values
        savedArchive
            .Should()
            .BeEquivalentTo(
                expectedArchive,
                options =>
                    options
                        .Excluding(x => x.Id)
                        .Excluding(x => x.CreatedAt)
                        .Excluding(x => x.WhitePlayerId)
                        .Excluding(x => x.WhitePlayer!.Id)
                        .Excluding(x => x.BlackPlayerId)
                        .Excluding(x => x.BlackPlayer!.Id)
                        .For(x => x.Moves)
                        .Exclude(x => x.Id)
            );
    }

    private static GameState CreateTestGameState() =>
        new(
            Fen: "10/10/10/10/10/10/10/10/10/10",
            WhitePlayer: new GamePlayerFaker(GameColor.White).Generate(),
            BlackPlayer: new GamePlayerFaker(GameColor.Black).Generate(),
            MoveHistory: ["e2e4", "e7e5", "g1f3"],
            SideToMove: GameColor.Black,
            LegalMoves: [],
            TimeControl: new(600, 5)
        );

    private async Task<GameArchive> GetSavedArchiveAsync(string gameToken)
    {
        var archive = await DbContext
            .GameArchives.Include(g => g.Moves)
            .Include(g => g.WhitePlayer)
            .Include(g => g.BlackPlayer)
            .FirstOrDefaultAsync(g => g.GameToken == gameToken, CT);

        archive.Should().NotBeNull();
        return archive;
    }

    private static PlayerArchive CreateExpectedPlayerArchive(GamePlayer player, int ratingDelta) =>
        new()
        {
            UserId = player.UserId,
            UserName = player.UserName,
            CountryCode = player.CountryCode,
            Color = player.Color,
            InitialRating = player.Rating,
            NewRating = player.Rating + ratingDelta,
        };

    private static IEnumerable<MoveArchive> CreateExpectedMoveArchives(
        IEnumerable<string> moveHistory
    ) =>
        moveHistory.Select(
            (move, index) => new MoveArchive { EncodedMove = move, MoveNumber = index }
        );
}
