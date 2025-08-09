namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.MoveSnapshot")]
public record MoveSnapshot(MovePath Path, string San, double TimeLeft);
