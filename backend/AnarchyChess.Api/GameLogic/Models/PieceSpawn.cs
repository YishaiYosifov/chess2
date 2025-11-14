namespace AnarchyChess.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameLogic.Models.PieceSpawn")]
public record PieceSpawn(PieceType Type, GameColor? Color, AlgebraicPoint Position);
