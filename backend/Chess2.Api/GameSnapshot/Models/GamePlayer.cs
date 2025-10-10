using Chess2.Api.GameLogic.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.GamePlayer")]
public record GamePlayer(
    UserId UserId,
    bool IsAuthenticated,
    GameColor Color,
    string UserName,
    string CountryCode,
    int? Rating
);
