namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.MoveOptions")]
public record MoveOptions(IReadOnlyCollection<MovePath> LegalMoves, bool HasForcedMoves)
{
    public MoveOptions()
        : this([], false) { }
}
