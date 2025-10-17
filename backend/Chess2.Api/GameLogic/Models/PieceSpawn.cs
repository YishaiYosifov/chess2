namespace Chess2.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameLogic.Models.PieceSpawn")]
public record PieceSpawn(PieceType Type, GameColor? Color, AlgebraicPoint Position);
