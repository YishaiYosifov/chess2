using Chess2.Api.QuestLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.Shared.Services;

namespace Chess2.Api.Quests.Services;

public interface IRandomQuestProvider
{
    QuestInstance GetRandomQuestInstance(QuestInstance? except);
}

public class RandomQuestProvider(
    IEnumerable<IQuestDefinition> quests,
    TimeProvider timeProvider,
    IRandomProvider randomProvider
) : IRandomQuestProvider
{
    private readonly Dictionary<QuestDifficulty, List<QuestVariant>> _questVariantsByDifficulty =
        quests
            .SelectMany(x => x.Variants)
            .GroupBy(x => x.Difficulty)
            .ToDictionary(x => x.Key, x => x.ToList());

    private readonly Dictionary<DayOfWeek, QuestDifficulty> _dayDifficulty = new()
    {
        [DayOfWeek.Monday] = QuestDifficulty.Easy,
        [DayOfWeek.Tuesday] = QuestDifficulty.Easy,
        [DayOfWeek.Wednesday] = QuestDifficulty.Medium,
        [DayOfWeek.Thursday] = QuestDifficulty.Medium,
        [DayOfWeek.Friday] = QuestDifficulty.Medium,
        [DayOfWeek.Saturday] = QuestDifficulty.Hard,
        [DayOfWeek.Sunday] = QuestDifficulty.Hard,
    };

    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IRandomProvider _random = randomProvider;

    public QuestInstance GetRandomQuestInstance(QuestInstance? except)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        var difficulty = _dayDifficulty[today.DayOfWeek];

        var availableQuestVariants = _questVariantsByDifficulty[difficulty]
            .Where(x => x.Description != except?.Description);
        var questVariant = _random.NextItem(availableQuestVariants);

        var questInstance = questVariant.CreateInstance(creationDate: today);
        return questInstance;
    }
}
