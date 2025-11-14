using System.Diagnostics.CodeAnalysis;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Game.Services;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Services.PlayerRoster")]
public record PlayerRoster(GamePlayer WhitePlayer, GamePlayer BlackPlayer)
{
    public bool TryGetPlayerById(UserId? userId, [NotNullWhen(true)] out GamePlayer? player)
    {
        player = GetPlayerById(userId);
        return player is not null;
    }

    public GamePlayer? GetPlayerById(UserId? userId)
    {
        if (WhitePlayer.UserId == userId)
            return WhitePlayer;
        else if (BlackPlayer.UserId == userId)
            return BlackPlayer;
        return null;
    }

    public GamePlayer GetPlayerByColor(GameColor color) =>
        color.Match(whenWhite: WhitePlayer, whenBlack: BlackPlayer);
}
