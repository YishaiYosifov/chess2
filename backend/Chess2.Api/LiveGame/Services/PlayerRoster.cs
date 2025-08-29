using System.Diagnostics.CodeAnalysis;
using Chess2.Api.GameLogic.Extensions;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Services;

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Services.PlayerRoster")]
public record PlayerRoster(GamePlayer WhitePlayer, GamePlayer BlackPlayer)
{
    public bool TryGetPlayerById(string userId, [NotNullWhen(true)] out GamePlayer? player)
    {
        player = null;
        if (WhitePlayer.UserId == userId)
            player = WhitePlayer;
        else if (BlackPlayer.UserId == userId)
            player = BlackPlayer;

        return player is not null;
    }

    public GamePlayer GetPlayerByColor(GameColor color) =>
        color.Match(whenWhite: WhitePlayer, whenBlack: BlackPlayer);
}
