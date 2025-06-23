using System.ComponentModel;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Newtonsoft.Json;

namespace Chess2.Api.Game.DTOs;

[DisplayName("GamePlayer")]
[method: JsonConstructor]
public record GamePlayerDto(
    string UserId,
    GameColor Color,
    string UserName,
    string? CountryCode,
    int? Rating
)
{
    public GamePlayerDto(GamePlayer gamePlayer, string userName, string? countryCode, int? rating)
        : this(gamePlayer.UserId, gamePlayer.Color, userName, countryCode, rating) { }
}
