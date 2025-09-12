using Chess2.Api.Quests.Models;

namespace Chess2.Api.Quests.QuestProgressors;

public interface IQuestProgressor
{
    int EvaluateProgressMade(GameQuestSnapshot snapshot);
}
