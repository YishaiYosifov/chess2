using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Entities;
using Chess2.Api.Quests.Repositories;
using Chess2.Api.Quests.Services;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.QuestTests;

public class QuestServiceTests : BaseIntegrationTest
{
    private readonly QuestService _questService;
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    public QuestServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        _questService = new(
            Scope.ServiceProvider.GetRequiredService<IQuestRepository>(),
            _timeProviderMock,
            Scope.ServiceProvider.GetRequiredService<UserManager<AuthedUser>>(),
            Scope.ServiceProvider.GetRequiredService<IUnitOfWork>()
        );
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_applies_pagination()
    {
        List<UserQuestPoints> questPoints =
        [
            new UserQuestPointsFaker().RuleFor(x => x.Points, 4).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.Points, 3).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.Points, 2).Generate(),
            new UserQuestPointsFaker().RuleFor(x => x.Points, 1).Generate(),
        ];
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 3);

        var result = await _questService.GetPaginatedLeaderboardAsync(pagination, CT);

        var expected = questPoints[..3]
            .Select(x => new QuestPointsDto(new MinimalProfile(x.User), x.Points))
            .ToList();
        result.Items.Should().BeEquivalentTo(expected);
        result.TotalCount.Should().Be(questPoints.Count);
        result.Page.Should().Be(pagination.Page);
        result.PageSize.Should().Be(pagination.PageSize);
    }

    [Fact]
    public async Task GetRankingAsync_finds_correct_ranking()
    {
        var inMonthUsers = new UserQuestPointsFaker()
            .RuleFor(x => x.LastQuestAt, _fakeNow.UtcDateTime)
            .Generate(5);

        var outMonthUser = new UserQuestPointsFaker()
            .RuleFor(x => x.Points, 999)
            .RuleFor(x => x.LastQuestAt, _fakeNow.UtcDateTime.AddMinutes(-1))
            .Generate();

        await DbContext.AddRangeAsync(inMonthUsers, CT);
        await DbContext.AddAsync(outMonthUser, CT);
        await DbContext.SaveChangesAsync(CT);

        var testPoints = inMonthUsers[2];
        var result = await _questService.GetRankingAsync(testPoints.UserId, CT);

        result.Should().Be(inMonthUsers.Count(u => u.Points > testPoints.Points) + 1);
    }

    [Fact]
    public async Task GetQuestPointsAsync_returns_points_when_found()
    {
        var existing = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(existing, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _questService.GetQuestPointsAsync(existing.UserId, CT);

        result.Should().Be(existing.Points);
    }

    [Fact]
    public async Task GetQuestPointsAsync_returns_zero_when_no_points()
    {
        var existing = new UserQuestPointsFaker()
            .RuleFor(x => x.LastQuestAt, _fakeNow.UtcDateTime.AddMonths(-1))
            .Generate();
        await DbContext.AddAsync(existing, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _questService.GetQuestPointsAsync(existing.UserId, CT);

        result.Should().Be(0);
    }

    [Fact]
    public async Task IncrementQuestPointsAsync_adds_when_not_found()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);
        int points = 123;

        var result = await _questService.IncrementQuestPointsAsync(user.Id, points, CT);

        result.IsError.Should().BeFalse();

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new UserQuestPoints
                {
                    UserId = user.Id,
                    User = user,
                    Points = points,
                    LastQuestAt = _fakeNow.DateTime,
                },
                options => options.Excluding(x => x.Id)
            );
    }

    [Fact]
    public async Task IncrementQuestPointsAsync_updates_when_found()
    {
        var existing = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(existing, CT);
        await DbContext.SaveChangesAsync(CT);

        int incrementBy = 100;

        var result = await _questService.IncrementQuestPointsAsync(
            existing.UserId,
            incrementBy,
            CT
        );

        result.IsError.Should().BeFalse();
        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(
                new UserQuestPoints
                {
                    UserId = existing.UserId,
                    User = existing.User,
                    Points = existing.Points + incrementBy,
                    LastQuestAt = _fakeNow.DateTime,
                },
                options => options.Excluding(x => x.Id)
            );
    }
}
