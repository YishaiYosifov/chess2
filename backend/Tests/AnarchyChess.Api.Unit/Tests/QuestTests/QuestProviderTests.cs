using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestDefinitions;
using AnarchyChess.Api.Quests.Services;
using AnarchyChess.Api.Shared.Services;
using AwesomeAssertions;
using NSubstitute;

namespace AnarchyChess.Api.Unit.Tests.QuestTests;

public class RandomQuestProviderTests
{
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IRandomProvider _randomProviderMock = Substitute.For<IRandomProvider>();
    private readonly IQuestDefinition _questDefinitionMock = Substitute.For<IQuestDefinition>();
    private readonly DateTime _sunday = new(2025, 9, 14);

    private static QuestVariant MakeVariant(QuestDifficulty difficulty, string description)
    {
        QuestVariant variant = new(description, difficulty, Target: 0, Conditions: () => []);
        return variant;
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, QuestDifficulty.Easy)]
    [InlineData(DayOfWeek.Tuesday, QuestDifficulty.Easy)]
    [InlineData(DayOfWeek.Wednesday, QuestDifficulty.Medium)]
    [InlineData(DayOfWeek.Thursday, QuestDifficulty.Medium)]
    [InlineData(DayOfWeek.Friday, QuestDifficulty.Medium)]
    [InlineData(DayOfWeek.Saturday, QuestDifficulty.Hard)]
    [InlineData(DayOfWeek.Sunday, QuestDifficulty.Hard)]
    public void GetRandomQuestInstance_picks_quest_based_on_day_of_week(
        DayOfWeek day,
        QuestDifficulty expectedDifficulty
    )
    {
        var variant = MakeVariant(expectedDifficulty, "Test Quest");
        _questDefinitionMock.Variants.Returns([variant]);

        DateTimeOffset dayOfWeekDate = new(_sunday.AddDays((int)day));
        _timeProviderMock.GetUtcNow().Returns(dayOfWeekDate);
        _randomProviderMock.NextItem(Arg.Any<IEnumerable<QuestVariant>>()).Returns(variant);

        RandomQuestProvider questProvider = new(
            [_questDefinitionMock],
            _timeProviderMock,
            _randomProviderMock
        );

        var result = questProvider.GetRandomQuestInstance(null);

        result.CreationDate.ToDateTime(TimeOnly.MinValue).Should().Be(dayOfWeekDate.Date);
        variant
            .CreateInstance(DateOnly.FromDateTime(dayOfWeekDate.DateTime))
            .Should()
            .BeEquivalentTo(result);
    }

    [Fact]
    public void GetRandomQuestInstance_excludes_except_quest()
    {
        var quest1 = MakeVariant(QuestDifficulty.Hard, "Quest 1");
        var quest2 = MakeVariant(QuestDifficulty.Hard, "Quest 2");

        _questDefinitionMock.Variants.Returns([quest1, quest2]);
        _timeProviderMock.GetUtcNow().Returns(_sunday);
        _randomProviderMock
            .NextItem(
                Arg.Is<IEnumerable<QuestVariant>>(c => c.Contains(quest2) && !c.Contains(quest1))
            )
            .Returns(quest2);

        RandomQuestProvider questProvider = new(
            [_questDefinitionMock],
            _timeProviderMock,
            _randomProviderMock
        );

        var date = DateOnly.FromDateTime(_sunday);
        var exceptInstance = quest1.CreateInstance(date);

        var result = questProvider.GetRandomQuestInstance(exceptInstance);

        quest2.CreateInstance(date).Should().BeEquivalentTo(result);
    }
}
