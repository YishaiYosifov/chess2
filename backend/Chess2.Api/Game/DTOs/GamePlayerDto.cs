using System.ComponentModel;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Newtonsoft.Json;

namespace Chess2.Api.Game.DTOs;

[DisplayName("GamePlayer")]
[method: JsonConstructor]
public record GamePlayerDto(string UserId, GameColor Color, string UserName, int? Rating)
{
    public GamePlayerDto(GamePlayer gamePlayer, string UserName, int? Rating)
        : this(gamePlayer.UserId, gamePlayer.Color, UserName, Rating) { }
}
