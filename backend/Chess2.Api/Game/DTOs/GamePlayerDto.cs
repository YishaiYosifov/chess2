using System.ComponentModel;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.DTOs;

[DisplayName("GamePlayer")]
public record GamePlayerDto(string UserId, GameColor Color)
{
    public GamePlayerDto(GamePlayer gamePlayer)
        : this(gamePlayer.UserId, gamePlayer.Color) { }
}
