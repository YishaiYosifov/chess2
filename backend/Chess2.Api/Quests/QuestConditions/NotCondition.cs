using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestConditions.NotCondition")]
public class NotCondition(IQuestCondition inner) : IQuestCondition
{
    [Id(0)]
    private readonly IQuestCondition _inner = inner;

    public bool Evaluate(GameQuestSnapshot snapshot) => !_inner.Evaluate(snapshot);
}
