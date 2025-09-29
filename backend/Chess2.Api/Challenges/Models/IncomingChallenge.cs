using Chess2.Api.Profile.DTOs;

namespace Chess2.Api.Challenges.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Challenges.Models.IncomingChallenge")]
public record IncomingChallenge(
    ChallengeId ChallengeId,
    MinimalProfile Challenger,
    DateTime ExpiresAt
);
