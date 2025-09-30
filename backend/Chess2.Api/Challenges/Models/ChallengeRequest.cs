using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.DTOs;

namespace Chess2.Api.Challenges.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Challenges.Models.IncomingChallenge")]
public record ChallengeRequest(
    ChallengeId ChallengeId,
    MinimalProfile Requester,
    MinimalProfile Recipient,
    PoolKey Pool,
    DateTime ExpiresAt
);
