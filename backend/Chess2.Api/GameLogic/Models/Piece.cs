namespace Chess2.Api.GameLogic.Models;

public record Piece(PieceType Type, GameColor Color, int TimesMoved = 0);
