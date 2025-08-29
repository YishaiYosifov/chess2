namespace Chess2.Api.GameLogic.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameLogic.Models.MoveSideEffect")]
public record MoveSideEffect(AlgebraicPoint From, AlgebraicPoint To, Piece Piece);
