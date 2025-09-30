using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ChallengesTests;

public class ChallengeInboxGrainTests : BaseGrainTest
{
    private const string UserId = "user-123";

    [Fact]
    public async Task RecordChallengeCreatedAsync_adds_challenge_to_inbox()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        var challenge = new ChallengeRequestFaker().Generate();

        await grain.RecordChallengeCreatedAsync(challenge);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task RecordChallengeCreatedAsync_overwrites_existing_challenge()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        var challenge1 = new ChallengeRequestFaker().Generate();
        var challenge2 = new ChallengeRequestFaker()
            .RuleFor(x => x.ChallengeId, challenge1.ChallengeId)
            .Generate();

        await grain.RecordChallengeCreatedAsync(challenge1);
        await grain.RecordChallengeCreatedAsync(challenge2); // overwrite

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(challenge2);
    }

    [Fact]
    public async Task RecordChallengeCreatedAsync_removes_challenge_from_inbox()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        var challenge = new ChallengeRequestFaker().Generate();
        var someOtherChallenge = new ChallengeRequestFaker().Generate();

        await grain.RecordChallengeCreatedAsync(challenge);
        await grain.RecordChallengeCreatedAsync(someOtherChallenge);
        await grain.RecordChallengeRemovedAsync(challenge.ChallengeId);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(someOtherChallenge);
    }

    [Fact]
    public async Task RecordChallengeCreatedAsync_does_nothing_if_challenge_not_found()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        await grain.RecordChallengeRemovedAsync(new ChallengeId("nonexistent-challenge"));

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().BeEmpty();
    }

    [Fact]
    public async Task GetIncomingChallengesAsync_returns_all_challenges()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        ChallengeRequest challenge1 = new ChallengeRequestFaker().Generate();
        ChallengeRequest challenge2 = new ChallengeRequestFaker().Generate();

        await grain.RecordChallengeCreatedAsync(challenge1);
        await grain.RecordChallengeCreatedAsync(challenge2);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().BeEquivalentTo([challenge1, challenge2]);
    }
}
