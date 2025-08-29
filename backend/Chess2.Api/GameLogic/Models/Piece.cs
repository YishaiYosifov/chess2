namespace Chess2.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameLogic.Models.Piece")]
public record Piece(PieceType Type, GameColor? Color, int TimesMoved = 0);
