using Chess2.Api.ArchivedGames.Entities;
using Chess2.Api.ArchivedGames.Models;
using Chess2.Api.ArchivedGames.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.Pagination.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.UserRating.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.ArchivedGamesTests;

public class GameArchiveServiceTests : BaseIntegrationTest
{
    private readonly IGameArchiveService _gameArchiveService;

    private readonly GameToken _gameToken = "test game token";

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
            _gameToken,
            gameState,
            endStatus,
            ratingChange,
            CT
        );

        await DbContext.SaveChangesAsync(CT);

        var savedArchive = await GetSavedArchiveAsync(_gameToken);

        savedArchive.Should().BeEquivalentTo(result);
        var expectedArchive = new GameArchive
        {
            GameToken = _gameToken,
            Result = endStatus.Result,
            ResultDescription = endStatus.ResultDescription,
            InitialFen = gameState.InitialFen,

            PoolType = gameState.Pool.PoolType,
            BaseSeconds = gameState.Pool.TimeControl.BaseSeconds,
            IncrementSeconds = gameState.Pool.TimeControl.IncrementSeconds,

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
            Moves = [.. CreateExpectedMoveArchives(gameState.MoveHistory)],
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
                        .Excluding(x => x.WhitePlayer.Id)
                        .Excluding(x => x.BlackPlayerId)
                        .Excluding(x => x.BlackPlayer.Id)
                        .For(x => x.Moves)
                        .Exclude(x => x.Id)
                        .For(x => x.Moves)
                        .For(x => x.SideEffects)
                        .Exclude(x => x.Id)
            );
    }

    [Fact]
    public async Task CreateArchiveAsync_saves_rating_even_if_rating_change_is_null()
    {
        var gameState = new GameStateFaker().Generate();
        GameEndStatus endStatus = new(GameResult.WhiteWin, "White Won by Resignation");

        var result = await _gameArchiveService.CreateArchiveAsync(
            _gameToken,
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

    [Fact]
    public async Task GetPaginatedResultsAsync_returns_expected_metadata_and_items()
    {
        var userId = "user123";
        var archives = new GameArchiveFaker(whiteUserId: userId).Generate(4);
        await DbContext.GameArchives.AddRangeAsync(archives, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 2);

        var result = await _gameArchiveService.GetPaginatedResultsAsync(userId, pagination, CT);

        result.Page.Should().Be(0);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(4);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(dto => archives.Any(a => a.GameToken == dto.GameToken));
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_returns_empty_when_user_has_no_archives()
    {
        PaginationQuery pagination = new(Page: 0, PageSize: 5);

        var result = await _gameArchiveService.GetPaginatedResultsAsync("no one", pagination, CT);

        result.Page.Should().Be(0);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaginatedResultsAsync_maps_all_properties_correctly()
    {
        var archive = new GameArchiveFaker().Generate();
        await DbContext.AddAsync(archive, CT);
        await DbContext.SaveChangesAsync(CT);

        var pagination = new PaginationQuery(Page: 0, PageSize: 1);
        var result = await _gameArchiveService.GetPaginatedResultsAsync(
            archive.BlackPlayer.UserId,
            pagination,
            CT
        );

        result.Items.Should().ContainSingle();
        var summary = result.Items.Single();

        GameSummaryDto expectedSummary = new(
            GameToken: archive.GameToken,
            WhitePlayer: new PlayerSummaryDto(
                UserId: archive.WhitePlayer.UserId,
                IsAuthenticated: archive.WhitePlayer.IsAuthenticated,
                UserName: archive.WhitePlayer.UserName,
                Rating: archive.WhitePlayer.NewRating
            ),
            BlackPlayer: new PlayerSummaryDto(
                UserId: archive.BlackPlayer.UserId,
                IsAuthenticated: archive.BlackPlayer.IsAuthenticated,
                UserName: archive.BlackPlayer.UserName,
                Rating: archive.BlackPlayer.NewRating
            ),
            Result: archive.Result,
            CreatedAt: archive.CreatedAt
        );
        summary.Should().BeEquivalentTo(expectedSummary);
    }

    private async Task<GameArchive> GetSavedArchiveAsync(GameToken gameToken)
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
            IsAuthenticated = player.IsAuthenticated,
            CountryCode = player.CountryCode,
            Color = player.Color,
            NewRating = player.Rating + ratingChange,
            RatingChange = ratingChange,
            FinalTimeRemaining = timeLeft,
        };

    private static IEnumerable<MoveArchive> CreateExpectedMoveArchives(
        IEnumerable<MoveSnapshot> moveHistory
    ) =>
        moveHistory.Select(
            (move, index) =>
                new MoveArchive
                {
                    TimeLeft = move.TimeLeft,
                    MoveNumber = index,
                    San = move.San,
                    MoveKey = move.Path.MoveKey,
                    FromIdx = move.Path.FromIdx,
                    ToIdx = move.Path.ToIdx,
                    Captures = move.Path.CapturedIdxs?.ToList() ?? [],
                    Triggers = move.Path.TriggerIdxs?.ToList() ?? [],
                    Intermediates = move.Path.IntermediateIdxs?.ToList() ?? [],
                    SideEffects =
                        move.Path.SideEffects?.Select(se => new MoveSideEffectArchive
                            {
                                FromIdx = se.FromIdx,
                                ToIdx = se.ToIdx,
                            })
                            .ToList() ?? [],
                    PromotesTo = move.Path.PromotesTo,
                }
        );
}
