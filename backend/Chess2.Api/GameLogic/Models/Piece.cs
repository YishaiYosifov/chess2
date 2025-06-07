namespace Chess2.Api.GameLogic.Models;

public record Piece(PieceType Type, Color Color, int TimesMoved = 0);
