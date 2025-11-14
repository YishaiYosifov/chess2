using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Matchmaking.Models;
using AnarchyChess.Api.Profile.DTOs;

namespace AnarchyChess.Api.Challenges.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Challenges.Models.IncomingChallenge")]
public record ChallengeRequest(
    ChallengeToken ChallengeToken,
    MinimalProfile Requester,
    MinimalProfile? Recipient,
    TimeControl TimeControl,
    PoolKey Pool,
    DateTime ExpiresAt
);
