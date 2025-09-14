using Chess2.Api.Pagination.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using System.Net;

namespace Chess2.Api.Functional.Tests;

public class QuestsControllerTests(Chess2WebApplicationFactory factory)
    : BaseFunctionalTest(factory)
{
    [Fact]
    public async Task GetDailyQuest_returns_a_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var quest1Response = await ApiClient.Api.GetDailyQuest();
        quest1Response.IsSuccessful.Should().BeTrue();

        var quest2Response = await ApiClient.Api.GetDailyQuest();
        quest2Response.IsSuccessful.Should().BeTrue();

        quest1Response.Content.Should().NotBeNull();
        quest1Response.Content.Should().BeEquivalentTo(quest2Response.Content);
    }

    [Fact]
    public async Task GetDailyQuest_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest");

        var response = await ApiClient.Api.GetDailyQuest();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReplaceDailyQuest_returns_a_new_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var quest1Response = await ApiClient.Api.GetDailyQuest();
        quest1Response.IsSuccessful.Should().BeTrue();

        var replaceResponse = await ApiClient.Api.ReplaceDailyQuest();
        replaceResponse.IsSuccessful.Should().BeTrue();

        replaceResponse.Content.Should().NotBeEquivalentTo(quest1Response.Content);

        var questAfterReplaceResponse = await ApiClient.Api.GetDailyQuest();
        questAfterReplaceResponse.IsSuccessful.Should().BeTrue();

        questAfterReplaceResponse.Content.Should().BeEquivalentTo(replaceResponse.Content);
    }

    [Fact]
    public async Task ReplaceDailyQuest_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest");

        var response = await ApiClient.Api.ReplaceDailyQuest();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CollectQuestReward_disallows_claiming_without_completing_a_quest()
    {
        await AuthUtils.AuthenticateAsync(ApiClient);

        var response = await ApiClient.Api.CollectQuestReward();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CollectQuestReward_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest");

        var response = await ApiClient.Api.CollectQuestReward();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetQuestLeaderboard_returns_public_users()
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

        var response = await ApiClient.Api.GetQuestLeaderboard(
            new PaginationQuery(Page: 0, PageSize: 3)
        );

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().NotBeNull();
        response.Content.TotalCount.Should().Be(users.Count);
        response
            .Content.Items.Should()
            .BeEquivalentTo(users[..3].Select(x => new QuestPointsDto(new(x), x.QuestPoints)));
    }

    [Fact]
    public async Task GetQuestLeaderboard_returns_bad_request_for_invalid_pagination()
    {
        var user = new AuthedUserFaker().Generate();
        await DbContext.AddAsync(user, CT);
        await DbContext.SaveChangesAsync(CT);

        var response = await ApiClient.Api.GetQuestLeaderboard(
            new PaginationQuery(Page: 0, PageSize: -1)
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMyQuestRanking_returns_user_ranking()
    {
        var user = new AuthedUserFaker().RuleFor(x => x.QuestPoints, 10).Generate();
        var higherUsers = new AuthedUserFaker().RuleFor(x => x.QuestPoints, 20).Generate(5);
        await DbContext.Users.AddAsync(user, CT);
        await DbContext.Users.AddRangeAsync(higherUsers, CT);
        await DbContext.SaveChangesAsync(CT);

        await AuthUtils.AuthenticateWithUserAsync(ApiClient, user);

        var response = await ApiClient.Api.GetMyQuestRanking();

        response.IsSuccessful.Should().BeTrue();
        response.Content.Should().Be(higherUsers.Count + 1);
    }

    [Fact]
    public async Task GetMyQuestRanking_rejects_unauthorized()
    {
        AuthUtils.AuthenticateGuest(ApiClient, "guest");

        var response = await ApiClient.Api.GetMyQuestRanking();

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
