using Chess2.Api.QuestLogic.Models;

namespace Chess2.Api.QuestLogic.QuestConditions;

public interface IQuestCondition
{
    bool Evaluate(GameQuestSnapshot snapshot);
}
