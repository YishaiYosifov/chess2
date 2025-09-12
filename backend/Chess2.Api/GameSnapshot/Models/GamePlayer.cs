using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.GamePlayer")]
public record GamePlayer(
    bool IsAuthenticated,
    string UserId,
    GameColor Color,
    string UserName,
    string CountryCode,
    int? Rating
);
