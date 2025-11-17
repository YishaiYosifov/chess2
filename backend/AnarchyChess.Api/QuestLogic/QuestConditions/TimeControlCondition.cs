using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.QuestLogic.Models;

namespace AnarchyChess.Api.QuestLogic.QuestConditions;

[GenerateSerializer]
[Alias("AnarchyChess.Api.QuestLogic.QuestConditions.TimeControlCondition")]
public class TimeControlCondition(TimeControlSettings timeControl) : IQuestCondition
{
    [Id(0)]
    private readonly TimeControlSettings _timeControl = timeControl;

    public bool Evaluate(GameQuestSnapshot snapshot) =>
        snapshot.FinalGameState.Pool.TimeControl == _timeControl;
}
