using Chess2.Api.Users.Models;

namespace Chess2.Api.Matchmaking.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Matchmaking.Models.SeekKey")]
public record SeekKey(UserId UserId, PoolKey Pool);
