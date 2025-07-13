using Chess2.Api.Game.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.GameTests.GameArchiveTests;

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
}
