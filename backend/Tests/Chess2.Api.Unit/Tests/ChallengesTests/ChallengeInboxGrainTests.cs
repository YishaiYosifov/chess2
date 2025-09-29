using Chess2.Api.Challenges.Grains;
using Chess2.Api.Challenges.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests.ChallengesTests;

public class ChallengeInboxGrainTests : BaseGrainTest
{
    private const string UserId = "user-123";

    [Fact]
    public async Task UserChallengedAsync_adds_challenge_to_inbox()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        IncomingChallenge challenge = new(
            new ChallengeId("challenge-1"),
            new MinimalProfileFaker().Generate(),
            DateTime.UtcNow.AddMinutes(5)
        );

        await grain.UserChallengedAsync(challenge);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(challenge);
    }

    [Fact]
    public async Task UserChallengedAsync_overwrites_existing_challenge()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        IncomingChallenge challenge1 = new(
            new ChallengeId("challenge-1"),
            new MinimalProfileFaker().Generate(),
            DateTime.UtcNow.AddMinutes(5)
        );

        IncomingChallenge challenge2 = new(
            new ChallengeId("challenge-1"),
            new MinimalProfileFaker().Generate(),
            DateTime.UtcNow.AddMinutes(10)
        );

        await grain.UserChallengedAsync(challenge1);
        await grain.UserChallengedAsync(challenge2); // overwrite

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().ContainSingle().Which.Should().BeEquivalentTo(challenge2);
    }

    [Fact]
    public async Task ChallengeCanceledAsync_removes_challenge_from_inbox()
    {
        var grain = await Silo.CreateGrainAsync<ChallengeInboxGrain>(UserId);

        IncomingChallenge challenge = new(
            new ChallengeId("challenge-1"),
            new MinimalProfileFaker().Generate(),
            DateTime.UtcNow.AddMinutes(5)
        );
        IncomingChallenge someOtherChallenge = new(
            new ChallengeId("something"),
            new MinimalProfileFaker().Generate(),
            DateTime.UtcNow
        );

        await grain.UserChallengedAsync(challenge);
        await grain.UserChallengedAsync(someOtherChallenge);
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

        IncomingChallenge challenge1 = new(
            new ChallengeId("challenge-1"),
            new MinimalProfileFaker().Generate(),
            DateTime.UtcNow.AddMinutes(5)
        );

        IncomingChallenge challenge2 = new(
            new ChallengeId("challenge-2"),
            new MinimalProfileFaker().Generate(),
            DateTime.UtcNow.AddMinutes(10)
        );

        await grain.UserChallengedAsync(challenge1);
        await grain.UserChallengedAsync(challenge2);

        var incoming = await grain.GetIncomingChallengesAsync();
        incoming.Should().BeEquivalentTo([challenge1, challenge2]);
    }
}
