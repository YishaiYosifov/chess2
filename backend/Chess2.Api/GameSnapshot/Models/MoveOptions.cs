namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.MoveOptions")]
public record MoveOptions(IReadOnlyCollection<MovePath> LegalMoves, bool HasForcedMoves)
{
    public MoveOptions()
        : this([], false) { }
}
