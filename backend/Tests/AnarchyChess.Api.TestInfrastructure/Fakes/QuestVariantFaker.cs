using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.TestInfrastructure.Fakes;

public class QuestVariantFaker : RecordFaker<QuestVariant>
{
    public QuestVariantFaker(
        List<IQuestCondition> conditions,
        List<IQuestMetric>? progressors = null,
        int? target = null
    )
    {
        StrictMode(true);
        RuleFor(x => x.Description, f => f.Lorem.Sentence());
        RuleFor(x => x.Difficulty, f => f.PickRandom<QuestDifficulty>());
        RuleFor(x => x.Target, f => target ?? f.Random.Int(1, 100));
        RuleFor(x => x.Conditions, () => conditions);
        RuleFor(x => x.Progressors, progressors is null ? null : () => progressors);
        RuleFor(x => x.ShouldResetOnFailure, false);
    }
}
