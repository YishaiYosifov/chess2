namespace Chess2.Api.GameSnapshot.Models;

public record MoveOptions(IReadOnlyCollection<MovePath> LegalMoves, bool HasForcedMoves)
{
    public MoveOptions()
        : this([], false) { }
}
