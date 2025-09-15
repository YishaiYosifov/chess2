using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.DTOs;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Chess2.Api.Integration.Tests.QuestTests;

public class QuestLeaderboardServiceTests : BaseIntegrationTest
{
    private readonly IQuestLeaderboardService _questLeaderboardService;

    public QuestLeaderboardServiceTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _questLeaderboardService =
            Scope.ServiceProvider.GetRequiredService<IQuestLeaderboardService>();
    }

    [Fact]
    public async Task GetPaginatedLeaderboardAsync_applies_pagination()
    {
        List<AuthedUser> users =
        [
            new AuthedUserFaker().RuleFor(x => x.QuestPoints, 4),
            new AuthedUserFaker().RuleFor(x => x.QuestPoints, 3),
            new AuthedUserFaker().RuleFor(x => x.QuestPoints, 2),
            new AuthedUserFaker().RuleFor(x => x.QuestPoints, 1),
        ];
        await DbContext.AddRangeAsync(users, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 3);

        var result = await _questLeaderboardService.GetPaginatedLeaderboardAsync(pagination, CT);

        var expected = users[..3]
            .Select(x => new QuestPointsDto(new MinimalProfile(x), x.QuestPoints))
            .ToList();
        result.Items.Should().BeEquivalentTo(expected);
        result.TotalCount.Should().Be(users.Count);
        result.Page.Should().Be(pagination.Page);
        result.PageSize.Should().Be(pagination.PageSize);
    }
}
