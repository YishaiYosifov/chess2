using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.Repositories;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.QuestTests;

public class QuestLeaderboardRepositoryTests : BaseIntegrationTest
{
    private readonly IQuestLeaderboardRepository _repository;

    public QuestLeaderboardRepositoryTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _repository = Scope.ServiceProvider.GetRequiredService<IQuestLeaderboardRepository>();
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_returns_correct_page_of_top_users()
    {
        int page = 1;
        int pageSize = 3;
        var users = await CreateHoleyUsersAsync();

        var result = await _repository.GetPaginatedLeaderboardAsync(
            new PaginationQuery(Page: page, PageSize: pageSize),
            CT
        );

        var expected = users
            .Where(x => x.QuestPoints > 0)
            .OrderByDescending(x => x.QuestPoints)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToList();

        result.Should().HaveCount(expected.Count);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetTotalCountAsync_returns_the_number_of_users_with_quest_points()
    {
        var users = await CreateHoleyUsersAsync();

        var result = await _repository.GetTotalCountAsync(CT);

        result.Should().Be(users.Count(x => x.QuestPoints > 0));
    }

    [Fact]
    public async Task GetUserRankingAsync_finds_user_position()
    {
        List<AuthedUser> users = [];
        for (var i = 10; i >= 0; i--)
        {
            users.Add(new AuthedUserFaker().RuleFor(x => x.QuestPoints, i));
        }
        await DbContext.AddRangeAsync(users, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetRankingAsync(users[3], CT);

        result.Should().Be(4);
    }

    private async Task<List<AuthedUser>> CreateHoleyUsersAsync()
    {
        var users = new List<AuthedUser>();
        for (int i = 1; i <= 10; i++)
        {
            // every 3rd user has 0 points
            int points = i % 3 == 0 ? 0 : i;
            users.Add(new AuthedUserFaker().RuleFor(x => x.QuestPoints, points));
        }
        await DbContext.AddRangeAsync(users, CT);
        await DbContext.SaveChangesAsync(CT);

        return users;
    }
}
