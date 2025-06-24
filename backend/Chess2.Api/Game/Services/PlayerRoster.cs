using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using System.Diagnostics.CodeAnalysis;

namespace Chess2.Api.Game.Services;

public interface IPlayerRoster
{
    GamePlayer BlackPlayer { get; }
    GamePlayer WhitePlayer { get; }

    void InitializePlayers(string whiteId, string blackId);
    bool TryGetPlayerByColor(GameColor color, [NotNullWhen(true)] out GamePlayer? player);
    bool TryGetPlayerById(string userId, [NotNullWhen(true)] out GamePlayer? player);
}

public class PlayerRoster(ILogger<PlayerRoster> logger) : IPlayerRoster
{
    public GamePlayer WhitePlayer =>
        _whitePlayer ?? throw new InvalidOperationException("White player is not set");
    public GamePlayer BlackPlayer =>
        _blackPlayer ?? throw new InvalidOperationException("Black player is not set");

    private Dictionary<GameColor, GamePlayer> _colorToPlayer = [];
    private Dictionary<string, GamePlayer> _idToPlayer = [];
    private GamePlayer? _whitePlayer;
    private GamePlayer? _blackPlayer;

    private readonly ILogger<PlayerRoster> _logger = logger;

    public void InitializePlayers(string whiteId, string blackId)
    {
        var whitePlayer = new GamePlayer(UserId: whiteId, Color: GameColor.White);
        var blackPlayer = new GamePlayer(UserId: blackId, Color: GameColor.Black);

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
