using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Quests.Repositories;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.QuestTests;

public class QuestRepositoryTests : BaseIntegrationTest
{
    private readonly IQuestRepository _repository;

    public QuestRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IQuestRepository>();
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_returns_correct_page_of_top_users()
    {
        int page = 1;
        int pageSize = 3;
        var questPoints = new UserQuestPointsFaker().Generate(10);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPaginatedLeaderboardAsync(
            new PaginationQuery(Page: page, PageSize: pageSize),
            CT
        );

        var expected = questPoints
            .OrderByDescending(x => x.Points)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        result.Should().HaveCount(expected.Count);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetTotalCountAsync_returns_the_number_of_users_with_quest_points()
    {
        var questPoints = new UserQuestPointsFaker().Generate(4);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTotalCountAsync(CT);
        result.Should().Be(questPoints.Count);
    }

    [Fact]
    public async Task GetUserRankingAsync_finds_user_position()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);

        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var testPoints = questPoints[2];
        var result = await _repository.GetRankingAsync(testPoints.Points, CT);

        result.Should().Be(questPoints.Count(u => u.Points > testPoints.Points) + 1);
    }

    [Fact]
    public async Task GetUserPointsAsync_finds_user_points()
    {
        var points = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(points, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetUserPointsAsync(points.UserId, CT);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(points);
    }

    [Fact]
    public async Task AddQuestPointsAsync_adds_points()
    {
        var newPoints = new UserQuestPointsFaker().Generate();

        await _repository.AddQuestPointsAsync(newPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should().ContainSingle().Which.Should().BeEquivalentTo(newPoints);
    }

    [Fact]
    public async Task DeleteAll_removes_all_points()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        _repository.DeleteAll();
        await DbContext.SaveChangesAsync(CT);

        (await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT)).Should().BeEmpty();
    }
}
