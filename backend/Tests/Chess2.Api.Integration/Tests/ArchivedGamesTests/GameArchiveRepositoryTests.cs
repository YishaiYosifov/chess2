using Chess2.Api.ArchivedGames.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.ArchivedGamesTests;

public class GameArchiveRepositoryTests : BaseIntegrationTest
{
    private readonly IGameArchiveRepository _gameArchiveRepository;

    public GameArchiveRepositoryTests(Chess2WebApplicationFactory factory)
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
    public async Task GetGameArchiveByToken_finds_archive_and_all_its_navigation_properties()
    {
        var gameArchive = new GameArchiveFaker().Generate();
        var otherGameArchive = new GameArchiveFaker().Generate();
        await DbContext.GameArchives.AddAsync(gameArchive, CT);
        await DbContext.GameArchives.AddAsync(otherGameArchive, CT);
        await DbContext.SaveChangesAsync(CT);
        DbContext.ChangeTracker.Clear();

        var result = await _gameArchiveRepository.GetGameArchiveByTokenAsync(
            gameArchive.GameToken,
            CT
        );

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(gameArchive);
    }

    [Fact]
    public async Task GetGameArchiveByToken_returns_null_when_archive_is_not_found()
    {
        var gameArchive = new GameArchiveFaker().Generate();
        await DbContext.GameArchives.AddAsync(gameArchive, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _gameArchiveRepository.GetGameArchiveByTokenAsync(
            "some random token",
            CT
        );

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPaginatedArchivedGamesForUserAsync_skips_and_takes_correct_number_of_items()
    {
        var userId = "user123";
        var archives = new GameArchiveFaker(whiteUserId: userId).Generate(5);
        await DbContext.AddRangeAsync(archives, CT);
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
