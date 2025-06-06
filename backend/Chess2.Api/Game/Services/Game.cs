using Chess2.Api.GameLogic;

namespace Chess2.Api.Game.Services;

public interface IGame { }

public class Game : IGame
{
    private readonly ChessBoard _board = new(
        GameConstants.StartingPosition,
        GameConstants.BoardHeight,
        GameConstants.BoardWidth
    );
}
