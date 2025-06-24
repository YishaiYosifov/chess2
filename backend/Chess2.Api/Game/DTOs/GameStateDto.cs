using Chess2.Api.Game.Models;
using Chess2.Api.GameLogic.Models;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Chess2.Api.Game.DTOs;

[DisplayName("GameState")]
[method: JsonConstructor]
public record GameStateDto(
    GamePlayerDto WhitePlayer,
    GamePlayerDto BlackPlayer,
    GameColor SideToMove,
    string Fen,
    IReadOnlyCollection<string> MoveHistory,
    IReadOnlyCollection<string> LegalMoves,
    TimeControlSettings TimeControl
)
{
    public GameStateDto(
        GamePlayerDto whitePlayerDto,
        GamePlayerDto blackPlayerDto,
        GameState gameState
    )
        : this(
            whitePlayerDto,
            blackPlayerDto,
            gameState.SideToMove,
            gameState.Fen,
            gameState.MoveHistory,
            gameState.LegalMoves,
            gameState.TimeControl
        )
    { }
}
