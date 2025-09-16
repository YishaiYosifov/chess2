using Chess2.Api.Pagination.Models;
using Chess2.Api.Quests.Entities;
using Chess2.Api.Quests.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.QuestTests;

public class QuestRepositoryTests : BaseIntegrationTest
{
    private readonly IQuestRepository _repository;

    public QuestRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IQuestRepository>();
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_returns_correct_page_of_top_users()
    {
        int page = 1;
        int pageSize = 3;
        var asOfMonth = DateTime.UtcNow;
        var users = await CreateHoleyUsersAsync(asOfMonth);

        var result = await _repository.GetPaginatedLeaderboardAsync(
            new PaginationQuery(Page: page, PageSize: pageSize),
            asOfMonth,
            CT
        );

        var expected = users
            .Where(x => x.Points > 0)
            .OrderByDescending(x => x.Points)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        result.Should().HaveCount(expected.Count);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_filters_by_asOfMonth()
    {
        var targetMonth = DateTime.UtcNow;
        var otherMonth = targetMonth.AddMonths(-1);

        var inMonthUsers = new UserQuestPointsFaker()
            .RuleFor(x => x.LastQuestAt, targetMonth)
            .Generate(5);

        var outMonthUsers = new UserQuestPointsFaker()
            .RuleFor(x => x.LastQuestAt, otherMonth)
            .Generate(3);

        await DbContext.AddRangeAsync(inMonthUsers.Concat(outMonthUsers), CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetPaginatedLeaderboardAsync(
            new PaginationQuery(Page: 0, PageSize: 10),
            targetMonth,
            CT
        );

        result.Should().HaveCount(inMonthUsers.Count);
        result.Should().BeEquivalentTo(inMonthUsers.OrderByDescending(x => x.Points));
    }

    [Fact]
    public async Task GetTotalCountAsync_returns_the_number_of_users_with_quest_points()
    {
        var targetMonth = DateTime.UtcNow;
        var otherMonth = targetMonth.AddMonths(-1);

        var inMonthUsers = new UserQuestPointsFaker()
            .RuleFor(x => x.LastQuestAt, targetMonth)
            .Generate(4);

        var outMonthUsers = new UserQuestPointsFaker()
            .RuleFor(x => x.LastQuestAt, otherMonth)
            .Generate(3);

        await DbContext.AddRangeAsync(inMonthUsers.Concat(outMonthUsers), CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTotalCountAsync(targetMonth, CT);
        result.Should().Be(inMonthUsers.Count);
    }

    [Fact]
    public async Task GetUserRankingAsync_finds_user_position()
    {
        var targetMonth = DateTime.UtcNow;
        var otherMonth = targetMonth.AddMonths(-1);

        var inMonthUsers = new UserQuestPointsFaker()
            .RuleFor(x => x.LastQuestAt, targetMonth)
            .Generate(5);

        var outMonthUser = new UserQuestPointsFaker()
            .RuleFor(x => x.Points, 999) // high score but wrong month
            .RuleFor(x => x.LastQuestAt, otherMonth)
            .Generate();

        await DbContext.AddRangeAsync(inMonthUsers, CT);
        await DbContext.AddAsync(outMonthUser, CT);
        await DbContext.SaveChangesAsync(CT);

        var testPoints = inMonthUsers[2];
        var result = await _repository.GetRankingAsync(testPoints.Points, targetMonth, CT);

        result.Should().Be(inMonthUsers.Count(u => u.Points > testPoints.Points) + 1);
    }

    [Fact]
    public async Task GetUserPointsAsync_finds_user_points()
    {
        var points = new UserQuestPointsFaker().Generate();
        await DbContext.AddAsync(points, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetUserPointsAsync(points.UserId, points.LastQuestAt, CT);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(points, options => options.Excluding(x => x.Id));
    }

    [Fact]
    public async Task GetUserPointsAsync_filters_out_by_month()
    {
        var today = DateTime.UtcNow;
        var points = new UserQuestPointsFaker()
            .RuleFor(x => x.LastQuestAt, today.AddMonths(-1))
            .Generate();
        await DbContext.AddAsync(points, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetUserPointsAsync(points.UserId, today, CT);
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddQuestPointsAsync_adds_points()
    {
        var newPoints = new UserQuestPointsFaker().Generate();

        await _repository.AddQuestPointsAsync(newPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        var inDb = await DbContext.QuestPoints.AsNoTracking().ToListAsync(CT);
        inDb.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(newPoints, options => options.Excluding(x => x.Id));
    }

    private async Task<List<UserQuestPoints>> CreateHoleyUsersAsync(DateTime month)
    {
        var userPoints = new List<UserQuestPoints>();
        for (int i = 1; i <= 10; i++)
        {
            // every 3rd user has 0 points
            int points = i % 3 == 0 ? 0 : i;
            userPoints.Add(
                new UserQuestPointsFaker()
                    .RuleFor(x => x.Points, points)
                    .RuleFor(x => x.LastQuestAt, month)
            );
        }
        await DbContext.AddRangeAsync(userPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        return userPoints;
    }
}
