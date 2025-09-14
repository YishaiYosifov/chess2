using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestConditions;

public interface IQuestCondition
{
    bool Evaluate(GameQuestSnapshot snapshot);
}
