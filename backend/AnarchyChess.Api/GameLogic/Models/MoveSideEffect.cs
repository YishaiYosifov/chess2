namespace AnarchyChess.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameLogic.Models.MoveSideEffect")]
public record MoveSideEffect(AlgebraicPoint From, AlgebraicPoint To, Piece Piece);
