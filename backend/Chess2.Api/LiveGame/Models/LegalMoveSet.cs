using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Models;

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Models.LegalMoveSet")]
public record LegalMoveSet(
    IReadOnlyDictionary<MoveKey, Move> MoveMap,
    IReadOnlyCollection<MovePath> MovePaths,
    IReadOnlyCollection<byte> EncodedMoves,
    bool HasForcedMoves = false
)
{
    public IEnumerable<Move> AllMoves => MoveMap.Values;

    public LegalMoveSet()
        : this(MoveMap: new Dictionary<MoveKey, Move>(), MovePaths: [], EncodedMoves: []) { }
}
