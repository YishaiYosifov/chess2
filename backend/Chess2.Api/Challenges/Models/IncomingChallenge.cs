using Chess2.Api.Profile.DTOs;

namespace Chess2.Api.Challenges.Models;

public record IncomingChallenge(
    ChallengeId ChallengeId,
    MinimalProfile Challenger,
    DateTime ExpiresAt
);
