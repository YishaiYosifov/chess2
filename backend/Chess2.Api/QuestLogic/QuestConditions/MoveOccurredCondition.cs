using Chess2.Api.GameLogic.Models;
using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("Chess2.Api.QuestLogic.QuestConditions.MoveOccurredCondition")]
public class MoveOccurredCondition(Func<Move, GameQuestSnapshot, bool> predicate) : IQuestCondition
{
    [Id(0)]
    private readonly Func<Move, GameQuestSnapshot, bool> _predicate = predicate;

    public bool Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.MoveHistory.Any(move => _predicate(move, snapshot));
}
