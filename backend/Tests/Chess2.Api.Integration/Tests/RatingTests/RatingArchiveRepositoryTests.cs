using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
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
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var archive = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(user).RuleFor(x => x.TimeControl, TimeControl.Rapid)
        );

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
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var archives = new RatingArchiveFaker(user, timeControl: TimeControl.Blitz).Generate(3);
        await DbContext.AddRangeAsync(archives, CT);

        var otherUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(otherUser, timeControl: TimeControl.Blitz)
        );

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
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

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
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        // low
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(user, rating: 1200, timeControl: TimeControl.Rapid)
        );
        // mid
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(user, rating: 1500, timeControl: TimeControl.Rapid)
        );
        var high = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(user, rating: 1800, timeControl: TimeControl.Rapid)
        );

        var otherUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(otherUser, rating: 2000, timeControl: TimeControl.Rapid)
        );

        var result = await _archiveRepository.GetHighestAsync(user.Id, TimeControl.Rapid, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(high);
    }

    [Fact]
    public async Task GetHighestAsync_returns_null_when_no_archives_exist()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var result = await _archiveRepository.GetHighestAsync(user.Id, TimeControl.Blitz, CT);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLowestAsync_returns_archive_with_min_value()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        // low
        var low = await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(user, rating: 1200, timeControl: TimeControl.Rapid)
        );
        // mid
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(user, rating: 1500, timeControl: TimeControl.Rapid)
        );
        // high
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(user, rating: 1800, timeControl: TimeControl.Rapid)
        );

        var otherUser = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());
        await FakerUtils.StoreFakerAsync(
            DbContext,
            new RatingArchiveFaker(otherUser, rating: 100, timeControl: TimeControl.Rapid)
        );

        var result = await _archiveRepository.GetLowestAsync(user.Id, TimeControl.Rapid, CT);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(low);
    }

    [Fact]
    public async Task GetLowestAsync_returns_null_when_no_archives_exist()
    {
        var user = await FakerUtils.StoreFakerAsync(DbContext, new AuthedUserFaker());

        var result = await _archiveRepository.GetLowestAsync(user.Id, TimeControl.Rapid, CT);

        result.Should().BeNull();
    }
}
