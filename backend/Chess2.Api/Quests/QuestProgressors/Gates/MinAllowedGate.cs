using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors.Gates;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Gates.MinAllowedGate")]
public class MinAllowedGate(IQuestProgressor inner, int minimumProgress) : IQuestProgressor
{
    [Id(0)]
    private readonly IQuestProgressor _inner = inner;

    [Id(1)]
    private readonly int _minimumProgress = minimumProgress;

    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor) =>
        _minimumProgress <= _inner.EvaluateProgressMade(snapshot, playerColor) ? 1 : 0;
}
