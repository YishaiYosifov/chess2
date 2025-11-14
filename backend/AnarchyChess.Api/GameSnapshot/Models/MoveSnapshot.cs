namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.MoveSnapshot")]
public record MoveSnapshot(MovePath Path, string San, double TimeLeft);
