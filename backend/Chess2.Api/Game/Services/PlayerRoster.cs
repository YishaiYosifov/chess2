using System.Diagnostics.CodeAnalysis;
using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.Game.Services;

public class PlayerRoster
{
    public GamePlayer WhitePlayer =>
        _whitePlayer ?? throw new InvalidOperationException("White player is not set");
    public GamePlayer BlackPlayer =>
        _blackPlayer ?? throw new InvalidOperationException("Black player is not set");

    private Dictionary<GameColor, GamePlayer> _colorToPlayer = [];
    private Dictionary<string, GamePlayer> _idToPlayer = [];
    private GamePlayer? _whitePlayer;
    private GamePlayer? _blackPlayer;

    public void InitializePlayers(GamePlayer whitePlayer, GamePlayer blackPlayer)
    {
        _whitePlayer = whitePlayer;
        _blackPlayer = blackPlayer;

        _idToPlayer = new Dictionary<string, GamePlayer>()
        {
            [whitePlayer.UserId] = whitePlayer,
            [blackPlayer.UserId] = blackPlayer,
        };

        _colorToPlayer = new Dictionary<GameColor, GamePlayer>()
        {
            [GameColor.White] = whitePlayer,
            [GameColor.Black] = blackPlayer,
        };
    }

    public bool TryGetPlayerById(string userId, [NotNullWhen(true)] out GamePlayer? player) =>
        _idToPlayer.TryGetValue(userId, out player);

    public bool TryGetPlayerByColor(GameColor color, [NotNullWhen(true)] out GamePlayer? player) =>
        _colorToPlayer.TryGetValue(color, out player);
}
