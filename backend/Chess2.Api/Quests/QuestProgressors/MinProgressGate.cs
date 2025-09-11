using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.MinProgressGate")]
public class MinProgressGate(IQuestProgressor inner, int minimumProgress) : IQuestProgressor
{
    [Id(0)]
    private readonly IQuestProgressor _inner = inner;

    [Id(1)]
    private readonly int _minimumProgress = minimumProgress;

    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor) =>
        _inner.EvaluateProgressMade(snapshot, playerColor) >= _minimumProgress ? 1 : 0;
}
