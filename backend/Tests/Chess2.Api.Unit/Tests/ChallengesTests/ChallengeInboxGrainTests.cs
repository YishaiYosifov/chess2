using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ChallengesTests;

public class ChallengeInboxGrainTests : BaseGrainTest
{
    private readonly UserId _userId = "user-123";

    [Fact]
    public async Task RecordChallengeCreatedAsync_adds_challenge_to_inbox()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(_userId);

        var challenge = new ChallengeRequestFaker().Generate();

        await grain.RecordChallengeCreatedAsync(challenge);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task RecordChallengeCreatedAsync_overwrites_existing_challenge()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(_userId);

        var challenge1 = new ChallengeRequestFaker().Generate();
        var challenge2 = new ChallengeRequestFaker()
            .RuleFor(x => x.ChallengeToken, challenge1.ChallengeToken)
            .Generate();

        await grain.RecordChallengeCreatedAsync(challenge1);
        await grain.RecordChallengeCreatedAsync(challenge2); // overwrite

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(challenge2);
    }

    [Fact]
    public async Task RecordChallengeCreatedAsync_removes_challenge_from_inbox()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(_userId);

        var challenge = new ChallengeRequestFaker().Generate();
        var someOtherChallenge = new ChallengeRequestFaker().Generate();

        await grain.RecordChallengeCreatedAsync(challenge);
        await grain.RecordChallengeCreatedAsync(someOtherChallenge);
        await grain.RecordChallengeRemovedAsync(challenge.ChallengeToken);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(someOtherChallenge);
    }

    [Fact]
    public async Task RecordChallengeCreatedAsync_does_nothing_if_challenge_not_found()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(_userId);

        await grain.RecordChallengeRemovedAsync(new ChallengeToken("nonexistent-challenge"));

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().BeEmpty();
    }

    [Fact]
    public async Task GetIncomingChallengesAsync_returns_all_challenges()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(_userId);

        ChallengeRequest challenge1 = new ChallengeRequestFaker().Generate();
        ChallengeRequest challenge2 = new ChallengeRequestFaker().Generate();

        await grain.RecordChallengeCreatedAsync(challenge1);
        await grain.RecordChallengeCreatedAsync(challenge2);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().BeEquivalentTo([challenge1, challenge2]);
    }
}
