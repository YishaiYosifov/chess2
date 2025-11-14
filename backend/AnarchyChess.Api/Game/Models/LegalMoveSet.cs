using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.LegalMoveSet")]
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
