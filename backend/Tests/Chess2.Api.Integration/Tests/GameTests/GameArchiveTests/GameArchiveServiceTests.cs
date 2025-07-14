using Chess2.Api.Game.Entities;
using Chess2.Api.Game.Models;
using Chess2.Api.Game.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.UserRating.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.GameTests.GameArchiveTests;

public class GameArchiveServiceTests : BaseIntegrationTest
{
    private readonly IGameArchiveService _gameArchiveService;

    private const string GameToken = "test game token";

    public GameArchiveServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _gameArchiveService = Scope.ServiceProvider.GetRequiredService<IGameArchiveService>();
    }

    [Fact]
    public async Task CreateArchiveAsync_creates_and_saves_the_game_archive_correctly()
    {
        var gameState = new GameStateFaker().Generate();
        GameEndStatus endStatus = new(GameResult.WhiteWin, "White Won by Resignation");
        var ratingChange = new RatingChange(WhiteChange: 100, BlackChange: -150);

        var result = await _gameArchiveService.CreateArchiveAsync(
            GameToken,
            gameState,
            endStatus,
            ratingChange,
            CT
        );

        await DbContext.SaveChangesAsync(CT);

        var savedArchive = await GetSavedArchiveAsync(GameToken);

        savedArchive.Should().BeEquivalentTo(result);
        var expectedArchive = new GameArchive
        {
            GameToken = GameToken,
            Result = endStatus.Result,
            ResultDescription = endStatus.ResultDescription,
            FinalFen = gameState.Fen,
            WhitePlayer = CreateExpectedPlayerArchive(
                gameState.WhitePlayer,
                ratingChange.WhiteChange,
                gameState.Clocks.WhiteClock
            ),
            BlackPlayer = CreateExpectedPlayerArchive(
                gameState.BlackPlayer,
                ratingChange.BlackChange,
                gameState.Clocks.BlackClock
            ),
            Moves = CreateExpectedMoveArchives(gameState.MoveHistory),
            IsRated = gameState.IsRated,
            BaseSeconds = gameState.TimeControl.BaseSeconds,
            IncrementSeconds = gameState.TimeControl.IncrementSeconds,
        };

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

    [Fact]
    public async Task CreateArchiveAsync_saves_rating_even_if_rating_change_is_null()
    {
        var gameState = new GameStateFaker().Generate();
        GameEndStatus endStatus = new(GameResult.WhiteWin, "White Won by Resignation");

        var result = await _gameArchiveService.CreateArchiveAsync(
            GameToken,
            gameState,
            endStatus,
            ratingChange: null,
            CT
        );

        result.WhitePlayer.RatingChange.Should().BeNull();
        result.WhitePlayer.NewRating.Should().Be(gameState.WhitePlayer.Rating);

        result.BlackPlayer.RatingChange.Should().BeNull();
        result.BlackPlayer.NewRating.Should().Be(gameState.BlackPlayer.Rating);
    }

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

    private static PlayerArchive CreateExpectedPlayerArchive(
        GamePlayer player,
        int ratingChange,
        double timeLeft
    ) =>
        new()
        {
            UserId = player.UserId,
            UserName = player.UserName,
            CountryCode = player.CountryCode,
            Color = player.Color,
            NewRating = player.Rating + ratingChange,
            RatingChange = ratingChange,
            FinalTimeRemaining = timeLeft,
        };

    private static List<MoveArchive> CreateExpectedMoveArchives(
        IEnumerable<MoveSnapshot> moveHistory
    ) =>
        [
            .. moveHistory.Select(
                (move, index) =>
                    new MoveArchive
                    {
                        EncodedMove = move.EncodedMove,
                        San = move.San,
                        TimeLeft = move.TimeLeft,
                        MoveNumber = index,
                    }
            ),
        ];
}
