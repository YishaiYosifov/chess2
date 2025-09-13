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
    public async Task GetTopQuestPointsAsync_finds_top_users()
    {
        List<AuthedUser> users = [];
        for (var i = 10; i > 0; i--)
        {
            users.Add(new AuthedUserFaker().RuleFor(x => x.QuestPoints, i));
        }
        await DbContext.AddRangeAsync(users, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTopQuestPointsAsync(top: 5, CT);

        result.Should().BeEquivalentTo(users[..5]);
    }

    [Fact]
    public async Task GetTopQuestPointAsync_returns_everyone_if_not_enough_users()
    {
        var users = new AuthedUserFaker().Generate(5);
        await DbContext.AddRangeAsync(users, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetTopQuestPointsAsync(10, CT);

        result.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetUserRankingAsync_finds_user_position()
    {
        List<AuthedUser> users = [];
        for (var i = 10; i > 0; i--)
        {
            users.Add(new AuthedUserFaker().RuleFor(x => x.QuestPoints, i));
        }
        await DbContext.AddRangeAsync(users, CT);
        await DbContext.SaveChangesAsync(CT);

        var result = await _repository.GetUserRankingAsync(users[3], CT);

        result.Should().Be(4);
    }
}
