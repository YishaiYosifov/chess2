using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Models;

public record GamePlayer(
    string UserId,
    GameColor Color,
    string UserName,
    string? CountryCode,
    int? Rating
);
