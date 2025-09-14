using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors.Gates;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.Gates.LessThanGate")]
public class LessThanEqualGate(IQuestProgressor inner, int lessThanEqual) : IQuestGate
{
    [Id(0)]
    private readonly IQuestProgressor _inner = inner;

    [Id(1)]
    private readonly int _lessThanEqual = lessThanEqual;

    public int EvaluateProgressMade(GameQuestSnapshot snapshot) =>
        _inner.EvaluateProgressMade(snapshot) <= _lessThanEqual ? 1 : 0;
}
