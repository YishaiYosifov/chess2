using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Quests.QuestProgressors;

public interface IQuestProgressor
{
    int EvaluateProgressMade(GameState snapshot, GameColor playerColor);
}
