using System.Net;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;

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
}
