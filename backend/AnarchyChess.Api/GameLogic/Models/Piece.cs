namespace AnarchyChess.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameLogic.Models.Piece")]
public record Piece(PieceType Type, GameColor? Color, int TimesMoved = 0);
