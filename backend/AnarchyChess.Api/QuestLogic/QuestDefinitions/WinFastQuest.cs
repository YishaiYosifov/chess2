using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;

namespace AnarchyChess.Api.QuestLogic.QuestDefinitions;

public class WinFastQuest : IQuestDefinition
{
    public IEnumerable<QuestVariant> Variants =>
        [
            new QuestVariant(
                Description: "Win a 5 minutes, 0 seconds increment (5 + 0) game while using under 1 minute and 30 seconds of your clock. Game must last at least 15 moves.",
                Difficulty: QuestDifficulty.Hard,
                Target: 1,
                Conditions: () =>

                    [
                        new WinCondition(),
                        new GreaterThanEqualCondition(new MoveCountMetric(), greaterThanEqual: 15),
                        new TimeControlCondition(
                            new TimeControlSettings(BaseSeconds: 300, IncrementSeconds: 0)
                        ),
                        new LessThanEqualCondition(
                            new TimeUsedMsMetric(),
                            lessThanEqual: 90 * 1000
                        ),
                    ]
            ),
        ];
}
