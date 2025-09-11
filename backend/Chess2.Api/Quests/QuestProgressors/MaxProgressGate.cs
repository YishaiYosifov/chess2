using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.MaxProgressGate")]
public class MaxProgressGate(IQuestProgressor inner, int maxProgress) : IQuestProgressor
{
    [Id(0)]
    private readonly IQuestProgressor _inner = inner;

    [Id(1)]
    private readonly int _maxProgress = maxProgress;

    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor) =>
        _inner.EvaluateProgressMade(snapshot, playerColor) <= _maxProgress ? 1 : 0;
}
