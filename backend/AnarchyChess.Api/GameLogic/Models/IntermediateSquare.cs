namespace AnarchyChess.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameLogic.Models.IntermediateSquare")]
public record IntermediateSquare(AlgebraicPoint Position, bool IsCapture);
