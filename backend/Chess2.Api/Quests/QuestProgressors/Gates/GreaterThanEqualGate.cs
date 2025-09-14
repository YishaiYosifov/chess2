using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Gates;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Gates.GreaterThanEqualGate")]
public class GreaterThanEqualGate(IQuestProgressor inner, int greaterThanEqual) : IQuestGate
{
    [Id(0)]
    private readonly IQuestProgressor _inner = inner;

    [Id(1)]
    private readonly int _greaterThanEqual = greaterThanEqual;

    public int EvaluateProgressMade(GameQuestSnapshot snapshot) =>
        _inner.EvaluateProgressMade(snapshot) >= _greaterThanEqual ? 1 : 0;
}
