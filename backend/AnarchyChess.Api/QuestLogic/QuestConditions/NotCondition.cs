using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestConditions.NotCondition")]
public class NotCondition(IQuestCondition inner) : IQuestCondition
{
    [Id(0)]
    private readonly IQuestCondition _inner = inner;

    public bool Evaluate(GameQuestSnapshot snapshot) => !_inner.Evaluate(snapshot);
}
