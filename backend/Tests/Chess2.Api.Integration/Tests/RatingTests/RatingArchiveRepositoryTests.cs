using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.UserRating.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.RatingTests;

public class RatingArchiveRepositoryTests : BaseIntegrationTest
{
    private readonly IRatingArchiveRepository _archiveRepository;

    public RatingArchiveRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _archiveRepository = Scope.ServiceProvider.GetRequiredService<IRatingArchiveRepository>();
    }

    [Fact]
    public async Task AddRatingAsync_adds_archive_to_context()
    {
        var user = new AuthedUserFaker().Generate();
        var archive = new RatingArchiveFaker(user)
            .RuleFor(x => x.TimeControl, TimeControl.Rapid)
            .Generate();
        await DbContext.AddRangeAsync(user, archive);
        await DbContext.SaveChangesAsync(CT);

        var dbArchive = await DbContext
            .RatingArchives.AsNoTracking()
            .SingleOrDefaultAsync(
                a =>
                    a.UserId == user.Id
                    && a.TimeControl == TimeControl.Rapid
                    && a.Value == archive.Value,
                CT
            );

        dbArchive.Should().NotBeNull();
        dbArchive.Should().BeEquivalentTo(archive);
    }

    [Fact]
    public async Task GetArchivesAsync_returns_archives_after_since_date_in_chronological_order()
    {
        var user = new AuthedUserFaker().Generate();
        var archives = new RatingArchiveFaker(user, timeControl: TimeControl.Blitz).Generate(3);
        var otherUser = new AuthedUserFaker().Generate();
        var otherArchive = new RatingArchiveFaker(
            otherUser,
            timeControl: TimeControl.Blitz
        ).Generate();
        await DbContext.AddRangeAsync(archives, CT);
        await DbContext.AddRangeAsync(user, otherUser, otherArchive);
        await DbContext.SaveChangesAsync(CT);

        var since = DateTime.UtcNow.AddDays(-7);

        var result = await _archiveRepository.GetArchivesAsync(
            user.Id,
            TimeControl.Blitz,
            since,
            CT
        );

        result.Should().BeInAscendingOrder(x => x.AchievedAt);
        result.Should().BeEquivalentTo(archives);
    }

    [Fact]
    public async Task GetArchivesAsync_returns_empty_when_none_found()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var archives = await _archiveRepository.GetArchivesAsync(
            user.Id,
            TimeControl.Classical,
            DateTime.UtcNow.AddYears(-1),
            CT
        );

        archives.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHighestAsync_returns_archive_with_max_value()
    {
        var user = new AuthedUserFaker().Generate();
        var otherUser = new AuthedUserFaker().Generate();

        var high = new RatingArchiveFaker(
            user,
            rating: 1800,
            timeControl: TimeControl.Rapid
        ).Generate();

        await DbContext.AddRangeAsync(
            user,
            //low
            new RatingArchiveFaker(user, rating: 1200, timeControl: TimeControl.Rapid).Generate(),
            // mid
            new RatingArchiveFaker(user, rating: 1500, timeControl: TimeControl.Rapid).Generate(),
            high,
            // other user
            otherUser,
            new RatingArchiveFaker(
                otherUser,
                rating: 2000,
                timeControl: TimeControl.Rapid
            ).Generate()
        );
        await DbContext.SaveChangesAsync(CT);

        var result = await _archiveRepository.GetHighestAsync(user.Id, TimeControl.Rapid, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(high);
    }

    [Fact]
    public async Task GetHighestAsync_returns_null_when_no_archives_exist()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _archiveRepository.GetHighestAsync(user.Id, TimeControl.Blitz, CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLowestAsync_returns_archive_with_min_value()
    {
        var user = new AuthedUserFaker().Generate();
        var otherUser = new AuthedUserFaker().Generate();

        var low = new RatingArchiveFaker(
            user,
            rating: 1200,
            timeControl: TimeControl.Rapid
        ).Generate();

        await DbContext.AddRangeAsync(
            user,
            low,
            // mid
            new RatingArchiveFaker(user, rating: 1500, timeControl: TimeControl.Rapid).Generate(),
            // high
            new RatingArchiveFaker(user, rating: 1800, timeControl: TimeControl.Rapid).Generate(),
            // other user
            otherUser,
            new RatingArchiveFaker(
                otherUser,
                rating: 100,
                timeControl: TimeControl.Rapid
            ).Generate()
        );
        await DbContext.SaveChangesAsync(CT);

        var result = await _archiveRepository.GetLowestAsync(user.Id, TimeControl.Rapid, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(low);
    }

    [Fact]
    public async Task GetLowestAsync_returns_null_when_no_archives_exist()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _archiveRepository.GetLowestAsync(user.Id, TimeControl.Rapid, CT);

        result.Should().BeNull();
    }
}
