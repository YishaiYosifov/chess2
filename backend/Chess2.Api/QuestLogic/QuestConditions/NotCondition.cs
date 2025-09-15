using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestConditions.NotCondition")]
public class NotCondition(IQuestCondition inner) : IQuestCondition
{
    [Id(0)]
    private readonly IQuestCondition _inner = inner;

    public bool Evaluate(GameQuestSnapshot snapshot) => !_inner.Evaluate(snapshot);
}
