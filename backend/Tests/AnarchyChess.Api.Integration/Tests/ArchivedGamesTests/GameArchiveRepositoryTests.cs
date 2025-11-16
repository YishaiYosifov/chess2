using AnarchyChess.Api.ArchivedGames.Repositories;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.ArchivedGamesTests;

public class GameArchiveRepositoryTests : BaseIntegrationTest
{
    private readonly IGameArchiveRepository _gameArchiveRepository;

    public GameArchiveRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _gameArchiveRepository = Scope.ServiceProvider.GetRequiredService<IGameArchiveRepository>();
    }

    [Fact]
    public async Task AddArchiveAsync_adds_the_archive_and_its_children()
    {
        var gameArchive = new GameArchiveFaker().Generate();

        await _gameArchiveRepository.AddArchiveAsync(gameArchive, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await DbContext
            .GameArchives.Include(g => g.Moves)
            .Include(g => g.WhitePlayer)
            .Include(g => g.BlackPlayer)
            .FirstOrDefaultAsync(g => g.GameToken == gameArchive.GameToken, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(gameArchive);
    }

    [Fact]
    public async Task GetPaginatedArchivedGamesForUserAsync_skips_and_takes_correct_number_of_items()
    {
        var userId = "user123";
        var archives = new GameArchiveFaker(whiteUserId: userId)
            .RuleFor(x => x.Result, f => f.PickRandomWithout(GameResult.Aborted))
            .Generate(5);
        var otherUserArchives = new GameArchiveFaker(whiteUserId: "other user")
            .RuleFor(x => x.Result, f => f.PickRandomWithout(GameResult.Aborted))
            .Generate(5);
        await DbContext.AddRangeAsync(archives, CT);
        await DbContext.AddRangeAsync(otherUserArchives, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _gameArchiveRepository.GetPaginatedArchivedGamesForUserAsync(
            userId,
            pagination: new(Page: 1, PageSize: 2),
            CT
        );

        result
            .Should()
            .BeEquivalentTo(archives.OrderByDescending(a => a.CreatedAt).Skip(2).Take(2));
        result.Should().BeInDescendingOrder(a => a.CreatedAt);
    }

    [Fact]
    public async Task GetPaginatedArchivedGamesForUserAsync_ignores_aborted_games()
    {
        var userId = "user123";

        var validArchives = new GameArchiveFaker(whiteUserId: userId)
            .RuleFor(x => x.Result, f => f.PickRandomWithout(GameResult.Aborted))
            .Generate(3);

        var abortedArchives = new GameArchiveFaker(whiteUserId: userId)
            .RuleFor(x => x.Result, GameResult.Aborted)
            .Generate(2);

        await DbContext.AddRangeAsync(validArchives, CT);
        await DbContext.AddRangeAsync(abortedArchives, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _gameArchiveRepository.GetPaginatedArchivedGamesForUserAsync(
            userId,
            pagination: new(Page: 0, PageSize: 10),
            CT
        );

        result.Should().HaveCount(validArchives.Count);
        result.Should().BeEquivalentTo(validArchives);
    }

    [Fact]
    public async Task CountArchivedGamesForUserAsync_returns_correct_count()
    {
        var userId = "user1";

        var archive1 = new GameArchiveFaker().Generate();
        archive1.WhitePlayer.UserId = userId;
        var archive2 = new GameArchiveFaker().Generate();
        archive2.BlackPlayer.UserId = userId;

        var otherArchive = new GameArchiveFaker().Generate();
        otherArchive.WhitePlayer.UserId = "otherUser";

        await DbContext.GameArchives.AddRangeAsync([archive1, archive2, otherArchive], CT);
        await DbContext.SaveChangesAsync(CT);

        var count = await _gameArchiveRepository.CountArchivedGamesForUserAsync(userId, CT);

        count.Should().Be(2);
    }
}
