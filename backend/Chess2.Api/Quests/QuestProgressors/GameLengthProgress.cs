using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors;

[GenerateSerializer]
[Alias("Chess2.Api.Quests.QuestProgressors.GameLengthCondition")]
public class GameLengthProgress : IQuestProgressor
{
    public int EvaluateProgressMade(GameState snapshot, GameColor playerColor) =>
        snapshot.MoveHistory.Count;
}
