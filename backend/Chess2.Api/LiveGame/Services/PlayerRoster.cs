using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.LiveGame.Services;

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Services.PlayerRoster")]
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
