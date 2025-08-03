using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

public record DrawState(
    GameColor? ActiveRequester,
    IReadOnlyDictionary<GameColor, int> DrawCooldown
)
{
    public DrawState()
        : this(ActiveRequester: null, new Dictionary<GameColor, int>().AsReadOnly()) { }
}
