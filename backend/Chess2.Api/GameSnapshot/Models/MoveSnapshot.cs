namespace Chess2.Api.GameSnapshot.Models;

public record MoveSnapshot(MovePath Path, string San, double TimeLeft);
