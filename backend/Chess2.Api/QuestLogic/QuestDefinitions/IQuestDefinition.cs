namespace Chess2.Api.QuestLogic.QuestDefinitions;

public interface IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants { get; }
}
