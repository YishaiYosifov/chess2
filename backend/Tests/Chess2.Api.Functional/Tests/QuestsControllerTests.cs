using System.Net;
using Chess2.Api.Pagination.Models;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Entities;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Functional.Tests;

public class QuestsControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetDailyQuest_returns_a_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var quest1Response = await ApiClient.Api.GetDailyQuestAsync();
        quest1Response.IsSuccessful.Should().BeTrue();

        var quest2Response = await ApiClient.Api.GetDailyQuestAsync();
        quest2Response.IsSuccessful.Should().BeTrue();

        quest1Response.Content.Should().NotBeNull();
        quest1Response.Content.Should().BeEquivalentTo(quest2Response.Content);
    }

    [Fact]
    public async Task GetDailyQuest_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest");

        var response = await ApiClient.Api.GetDailyQuestAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReplaceDailyQuest_returns_a_new_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var quest1Response = await ApiClient.Api.GetDailyQuestAsync();
        quest1Response.IsSuccessful.Should().BeTrue();

        var replaceResponse = await ApiClient.Api.ReplaceDailyQuestAsync();
        replaceResponse.IsSuccessful.Should().BeTrue();

        replaceResponse.Content.Should().NotBeEquivalentTo(quest1Response.Content);

        var questAfterReplaceResponse = await ApiClient.Api.GetDailyQuestAsync();
        questAfterReplaceResponse.IsSuccessful.Should().BeTrue();

        questAfterReplaceResponse.Content.Should().BeEquivalentTo(replaceResponse.Content);
    }

    [Fact]
    public async Task ReplaceDailyQuest_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest");

        var response = await ApiClient.Api.ReplaceDailyQuestAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CollectQuestReward_disallows_claiming_without_completing_a_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var response = await ApiClient.Api.CollectQuestRewardAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CollectQuestReward_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest");

        var response = await ApiClient.Api.CollectQuestRewardAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserQuestPoints_returns_correct_points()
    {
        var questPoints = new UserQuestPointsFaker().Generate();
        var otherQuestPoints = new UserQuestPointsFaker().Generate();
        await DbContext.AddRangeAsync(questPoints, otherQuestPoints);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetUserQuestPointsAsync(questPoints.UserId);

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().Be(questPoints.Points);
    }

    [Fact]
    public async Task GetQuestLeaderboard_returns_public_users()
    {
        List<UserQuestPoints> questPoints =
        [
            new UserQuestPointsFaker().RuleFor(x => x.Points, 4),
            new UserQuestPointsFaker().RuleFor(x => x.Points, 3),
            new UserQuestPointsFaker().RuleFor(x => x.Points, 2),
            new UserQuestPointsFaker().RuleFor(x => x.Points, 1),
        ];
        await DbContext.AddRangeAsync(questPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        PaginationQuery pagination = new(Page: 0, PageSize: 3);

        var response = await ApiClient.Api.GetQuestLeaderboardAsync(
            new PaginationQuery(Page: 0, PageSize: 3)
        );

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        response.Content.TotalCount.Should().Be(questPoints.Count);
        response
            .Content.Items.Should()
            .BeEquivalentTo(
                questPoints[..3].Select(x => new QuestPointsDto(new(x.User), x.Points))
            );
    }

    [Fact]
    public async Task GetQuestLeaderboard_returns_bad_request_for_invalid_pagination()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetQuestLeaderboardAsync(
            new PaginationQuery(Page: 0, PageSize: -1)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMyQuestRanking_returns_user_ranking()
    {
        var questPoints = new UserQuestPointsFaker().RuleFor(x => x.Points, 10).Generate();
        var higherPoints = new UserQuestPointsFaker().RuleFor(x => x.Points, 20).Generate(5);
        await DbContext.AddAsync(questPoints, CT);
        await DbContext.AddRangeAsync(higherPoints, CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, questPoints.User);

        var response = await ApiClient.Api.GetMyQuestRankingAsync();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().Be(higherPoints.Count + 1);
    }

    [Fact]
    public async Task GetMyQuestRanking_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest");

        var response = await ApiClient.Api.GetMyQuestRankingAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
