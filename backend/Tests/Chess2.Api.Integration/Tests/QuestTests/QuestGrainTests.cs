using Chess2.Api.GameLogic.Models;
using Chess2.Api.Profile.Models;
using Chess2.Api.QuestLogic;
using Chess2.Api.QuestLogic.Models;
using Chess2.Api.QuestLogic.QuestConditions;
using Chess2.Api.QuestLogic.QuestDefinitions;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Errors;
using Chess2.Api.Quests.Grains;
using Chess2.Api.Quests.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Orleans.TestKit.Storage;

namespace Chess2.Api.Integration.Tests.QuestTests;

public class QuestGrainTests : BaseOrleansIntegrationTest
{
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IRandomQuestProvider _randomQuestProviderMock =
        Substitute.For<IRandomQuestProvider>();
    private readonly IQuestService _questService;
    private readonly List<QuestVariant> _questVariants;

    private readonly TestStorageStats _stateStats;

    private DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private int _usedVariantIdx = 0;
    private QuestInstance? _lastInstance;

    public QuestGrainTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _questVariants =
        [
            .. ApiTestBase
                .Scope.ServiceProvider.GetRequiredService<IEnumerable<IQuestDefinition>>()
                .SelectMany(x => x.Variants),
        ];
        _questService = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IQuestService>();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        Silo.ServiceProvider.AddService(_randomQuestProviderMock);
        Silo.ServiceProvider.AddService(_questService);
        Silo.ServiceProvider.AddService(_timeProviderMock);

        Silo.StorageManager.GetStorage<QuestGrainStorage>(QuestGrain.StateName);
        _stateStats = Silo.StorageManager.GetStorageStats(QuestGrain.StateName)!;
    }

    private async Task<IQuestGrain> CreateGrainAsync(UserId? userId = null) =>
        await Silo.CreateGrainAsync<QuestGrain>(userId ?? "user1");

    private QuestVariant SetupSelectableVariant(DateTimeOffset? date = null)
    {
        var variant = _questVariants[_usedVariantIdx++];
        SetupVariantRandom(variant, date);
        return variant;
    }

    private void SetupWinVariant(
        QuestDifficulty difficulty,
        int target,
        DateTimeOffset? date = null
    ) =>
        SetupVariantRandom(
            new QuestVariant(
                Conditions: () => [new WinCondition()],
                Description: "desc",
                Target: target,
                Difficulty: difficulty
            ),
            date
        );

    private void SetupVariantRandom(QuestVariant variant, DateTimeOffset? date = null)
    {
        var instance = variant.CreateInstance(DateOnly.FromDateTime((date ?? _fakeNow).DateTime));

        var previousInstance = _lastInstance;
        _randomQuestProviderMock
            .GetRandomQuestInstance(
                ArgEx.FluentAssert<QuestInstance?>(x => x.Should().BeEquivalentTo(previousInstance))
            )
            .Returns(instance);
        _lastInstance = instance;
    }

    [Fact]
    public async Task GetQuestAsync_returns_a_quest()
    {
        var variant = SetupSelectableVariant();

        var grain = await CreateGrainAsync();
        var quest = await grain.GetQuestAsync();

        quest
            .Should()
            .BeEquivalentTo(
                new QuestDto(
                    QuestDifficulty.Easy,
                    variant.Description,
                    Target: variant.Target,
                    Progress: 0,
                    CanReplace: true,
                    Streak: 0,
                    RewardCollected: false
                )
            );

        _stateStats.Writes.Should().Be(1);
    }

    [Fact]
    public async Task GetQuestAsync_returns_same_quest_if_already_selected_today()
    {
        var variant1 = SetupSelectableVariant();
        var grain = await CreateGrainAsync();

        var quest1 = await grain.GetQuestAsync();

        var variant2 = SetupSelectableVariant();
        var quest2 = await grain.GetQuestAsync();

        variant1.Should().NotBeEquivalentTo(variant2);
        quest1.Should().BeEquivalentTo(quest2);
    }

    [Fact]
    public async Task GetQuestAsync_returns_a_new_quest_the_next_day()
    {
        SetupSelectableVariant();
        var grain = await CreateGrainAsync();
        var quest1 = await grain.GetQuestAsync();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));
        var variant2 = SetupSelectableVariant();
        var quest2 = await grain.GetQuestAsync();

        quest1.Should().NotBeEquivalentTo(quest2);
        quest2
            .Should()
            .BeEquivalentTo(
                new QuestDto(
                    variant2.Difficulty,
                    variant2.Description,
                    Target: variant2.Target,
                    Progress: 0,
                    CanReplace: true,
                    Streak: 0,
                    RewardCollected: false
                )
            );
    }

    [Fact]
    public async Task OnGameOverAsync_increments_progress()
    {
        SetupWinVariant(QuestDifficulty.Easy, target: 2);
        var snapshot = new GameQuestSnapshotFaker().RuleForWin(GameColor.White).Generate();

        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync();
        _stateStats.ResetCounts();

        await grain.OnGameOverAsync(snapshot);

        var quest = await grain.GetQuestAsync();
        quest.Progress.Should().Be(1);
        _stateStats.Writes.Should().Be(2);
    }

    [Fact]
    public async Task OnGameOverAsync_does_nothing_if_conditions_not_met()
    {
        SetupSelectableVariant();
        var snapshot = new GameQuestSnapshotFaker().RuleForLoss(GameColor.White).Generate();

        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync();

        await grain.OnGameOverAsync(snapshot);

        var quest = await grain.GetQuestAsync();
        quest.Progress.Should().Be(0);
    }

    [Fact]
    public async Task OnGameOverAsync_completes_quest_and_updates_state()
    {
        SetupWinVariant(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker().RuleForWin(GameColor.White).Generate();

        var grain = await CreateGrainAsync();
        var initialQuest = await grain.GetQuestAsync();

        await grain.OnGameOverAsync(snapshot);

        SetupSelectableVariant();
        var questAfterCompletion = await grain.GetQuestAsync();
        questAfterCompletion.Description.Should().Be(initialQuest.Description);
        questAfterCompletion.CanReplace.Should().BeFalse();
        questAfterCompletion.RewardCollected.Should().BeFalse();

        // quest replacement should still fail until a new quest is selected
        var replaceAttempt = await grain.ReplaceQuestAsync();
        replaceAttempt.IsError.Should().BeTrue();
        replaceAttempt.FirstError.Should().Be(QuestErrors.CanotReplace);
    }

    [Fact]
    public async Task OnGameOverAsync_increments_streak_across_multiple_days()
    {
        SetupWinVariant(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker().RuleForWin(GameColor.White).Generate();

        var grain = await CreateGrainAsync();

        // day 1
        await grain.GetQuestAsync();
        await grain.OnGameOverAsync(snapshot);
        var quest1 = await grain.GetQuestAsync();
        quest1.Streak.Should().Be(1);

        // day 2
        _fakeNow += TimeSpan.FromDays(1);
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        SetupWinVariant(QuestDifficulty.Easy, target: 1);
        await grain.GetQuestAsync();
        await grain.OnGameOverAsync(snapshot);
        var quest2 = await grain.GetQuestAsync();
        quest2.Streak.Should().Be(2);
    }

    [Fact]
    public async Task Streak_resets_if_a_day_is_missed()
    {
        SetupWinVariant(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker().RuleForWin(GameColor.White).Generate();

        var grain = await CreateGrainAsync();

        // day 1 complete quest
        await grain.GetQuestAsync();
        await grain.OnGameOverAsync(snapshot);
        (await grain.GetQuestAsync()).Streak.Should().Be(1);

        // skip a day
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(2));
        SetupWinVariant(QuestDifficulty.Easy, target: 1);
        var questAfterSkip = await grain.GetQuestAsync();

        questAfterSkip.Streak.Should().Be(0);
    }

    [Fact]
    public async Task ReplaceQuestAsync_replaces_quest_when_allowed()
    {
        SetupSelectableVariant();
        var grain = await CreateGrainAsync();

        await grain.GetQuestAsync();
        _stateStats.ResetCounts();

        var variant2 = SetupSelectableVariant();
        var result = await grain.ReplaceQuestAsync();

        result.IsError.Should().BeFalse();
        result
            .Value.Should()
            .BeEquivalentTo(
                new QuestDto(
                    Difficulty: variant2.Difficulty,
                    Description: variant2.Description,
                    Target: variant2.Target,
                    Progress: 0,
                    CanReplace: false,
                    RewardCollected: false,
                    Streak: 0
                )
            );
        _stateStats.Writes.Should().Be(1);

        var questAfter = await grain.GetQuestAsync();
        questAfter.Should().BeEquivalentTo(result.Value);
    }

    [Fact]
    public async Task ReplaceQuestAsync_fails_if_cannot_replace()
    {
        SetupSelectableVariant();
        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync();

        SetupSelectableVariant();
        var firstReplacement = await grain.ReplaceQuestAsync();

        SetupSelectableVariant();
        var secondReplacement = await grain.ReplaceQuestAsync();

        secondReplacement.IsError.Should().BeTrue();
        secondReplacement.FirstError.Should().Be(QuestErrors.CanotReplace);

        var questAfter = await grain.GetQuestAsync();
        questAfter.Should().BeEquivalentTo(firstReplacement.Value);
    }

    [Fact]
    public async Task GetQuestAsync_resets_can_replace()
    {
        SetupSelectableVariant();
        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync();

        SetupSelectableVariant();
        var replacement = await grain.ReplaceQuestAsync();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));

        SetupSelectableVariant();
        var questAfterReset = await grain.GetQuestAsync();
        questAfterReset.Should().NotBe(replacement);
        questAfterReset.CanReplace.Should().BeTrue();
    }

    [Fact]
    public async Task CollectRewardAsync_fails_if_no_reward_pending()
    {
        SetupSelectableVariant();
        var grain = await CreateGrainAsync();

        var result = await grain.CollectRewardAsync();

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(QuestErrors.NoRewardToCollect);
    }

    [Fact]
    public async Task CollectRewardAsync_applies_reward_once()
    {
        var questPoints = new UserQuestPointsFaker().Generate();
        await ApiTestBase.DbContext.AddAsync(questPoints, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        SetupWinVariant(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker().RuleForWin(GameColor.White).Generate();

        var grain = await CreateGrainAsync(questPoints.UserId);
        await grain.OnGameOverAsync(snapshot);

        var pointsGameOver = await _questService.GetQuestPointsAsync(
            questPoints.UserId,
            ApiTestBase.CT
        );
        pointsGameOver.Should().Be(0);

        var reward = await grain.CollectRewardAsync();
        reward.Value.Should().Be((int)QuestDifficulty.Easy);

        var pointsAfterClaim = await _questService.GetQuestPointsAsync(
            questPoints.UserId,
            ApiTestBase.CT
        );
        pointsAfterClaim.Should().Be((int)QuestDifficulty.Easy);

        var secondAttempt = await grain.CollectRewardAsync();
        secondAttempt.IsError.Should().BeTrue();
        secondAttempt.FirstError.Should().Be(QuestErrors.NoRewardToCollect);
    }
}
