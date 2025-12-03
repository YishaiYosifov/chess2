using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Quests.Entities;
using AnarchyChess.Api.Quests.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.QuestTests;

public class QuestServiceTests : BaseIntegrationTest
{
    private readonly IQuestService _questService;

    public QuestServiceTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _questService = Scope.ServiceProvider.GetRequiredService<IQuestService>();
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
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var testPoints = questPoints[2];
        var result = await _questService.GetRankingAsync(testPoints.UserId, CT);

        result.Should().Be(questPoints.Count(u => u.Points > testPoints.Points) + 1);
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
        var existing = new UserQuestPointsFaker().Generate();
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
                }
            );
    }

    [Fact]
    public async Task IncrementQuestPointsAsync_updates_when_found()
    {
        var existing = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(existing, CT);
        await DbContext.SaveChangesAsync(CT);

        int incrementBy = 100;
        int expectedPoints = existing.Points + incrementBy;

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
                    Points = expectedPoints,
                }
            );
    }

    [Fact]
    public async Task ResetAllQuestPointsAsync_deletes_all()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        await _questService.ResetAllQuestPointsAsync(CT);

        (await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT)).Should().BeEmpty();
    }
}
