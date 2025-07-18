namespace Chess2.Api.GameSnapshot.Models;

public record MoveSnapshot(string EncodedMove, string San, double TimeLeft);
