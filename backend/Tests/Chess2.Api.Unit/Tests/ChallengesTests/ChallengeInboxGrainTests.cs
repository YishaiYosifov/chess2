using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ChallengesTests;

public class ChallengeInboxGrainTests : BaseGrainTest
{
    private const string UserId = "user-123";

    [Fact]
    public async Task ChallengeCreatedAsync_adds_challenge_to_inbox()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        var challenge = new IncomingChallengeFaker().Generate();

        await grain.ChallengeCreatedAsync(challenge);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task ChallengeCreatedAsync_overwrites_existing_challenge()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        var challenge1 = new IncomingChallengeFaker().Generate();
        var challenge2 = new IncomingChallengeFaker()
            .RuleFor(x => x.ChallengeId, challenge1.ChallengeId)
            .Generate();

        await grain.ChallengeCreatedAsync(challenge1);
        await grain.ChallengeCreatedAsync(challenge2); // overwrite

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(challenge2);
    }

    [Fact]
    public async Task ChallengeCanceledAsync_removes_challenge_from_inbox()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        var challenge = new IncomingChallengeFaker().Generate();
        var someOtherChallenge = new IncomingChallengeFaker().Generate();

        await grain.ChallengeCreatedAsync(challenge);
        await grain.ChallengeCreatedAsync(someOtherChallenge);
        await grain.ChallengeCanceledAsync(challenge.ChallengeId);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(someOtherChallenge);
    }

    [Fact]
    public async Task ChallengeCanceledAsync_does_nothing_if_challenge_not_found()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        await grain.ChallengeCanceledAsync(new ChallengeId("nonexistent-challenge"));

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().BeEmpty();
    }

    [Fact]
    public async Task GetIncomingChallengesAsync_returns_all_challenges()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        IncomingChallenge challenge1 = new IncomingChallengeFaker().Generate();
        IncomingChallenge challenge2 = new IncomingChallengeFaker().Generate();

        await grain.ChallengeCreatedAsync(challenge1);
        await grain.ChallengeCreatedAsync(challenge2);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().BeEquivalentTo([challenge1, challenge2]);
    }
}
