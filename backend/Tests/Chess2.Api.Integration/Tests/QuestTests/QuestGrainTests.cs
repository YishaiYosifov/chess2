using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.DTOs;
using Chess2.Api.Quests.Errors;
using Chess2.Api.Quests.Grains;
using Chess2.Api.Quests.Models;
using Chess2.Api.Quests.QuestDefinitions;
using Chess2.Api.Quests.QuestProgressors.Conditions;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.NSubtituteExtenstion;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Orleans.TestKit.Storage;

namespace Chess2.Api.Integration.Tests.QuestTests;

public class QuestGrainTests : BaseOrleansIntegrationTest
{
    private readonly IRandomProvider _randomMock = Substitute.For<IRandomProvider>();
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IEnumerable<IQuestDefinition> _quests;
    private readonly UserManager<AuthedUser> _userManager;

    private readonly HashSet<string> _usedVariantDescriptions = [];
    private readonly DateTimeOffset _fakeNow;

    public QuestGrainTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _userManager = ApiTestBase.Scope.ServiceProvider.GetRequiredService<
            UserManager<AuthedUser>
        >();
        _quests = ApiTestBase.Scope.ServiceProvider.GetRequiredService<
            IEnumerable<IQuestDefinition>
        >();

        _fakeNow = DateTimeOffset.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        Silo.ServiceProvider.AddService(_quests);
        Silo.ServiceProvider.AddService(_randomMock);
        Silo.ServiceProvider.AddService(_userManager);
        Silo.ServiceProvider.AddService(_timeProviderMock);
    }

    private async Task<IQuestGrain> CreateGrainAsync(string userId = "user1") =>
        await Silo.CreateGrainAsync<QuestGrain>(userId);

    private TestStorageStats? GetStorageStats() =>
        Silo.StorageManager.GetStorageStats<QuestGrain, QuestGrainStorage>();

    private void MockDifficulty(QuestDifficulty difficulty)
    {
        Dictionary<int, QuestDifficulty> expectedWeights = new()
        {
            [50] = QuestDifficulty.Easy,
            [30] = QuestDifficulty.Medium,
            [20] = QuestDifficulty.Hard,
        };
        _randomMock
            .NextWeighted(
                Arg.Is<IDictionary<int, QuestDifficulty>>(x => x.SequenceEqual(expectedWeights))
            )
            .Returns(difficulty);
    }

    private List<QuestVariant> GetFilteredVariants(QuestDifficulty difficulty)
    {
        return
        [
            .. _quests.SelectMany(quest =>
                quest.Variants.Where(variant =>
                    variant.Difficulty == difficulty
                    && !_usedVariantDescriptions.Contains(variant.Description)
                )
            ),
        ];
    }

    private QuestVariant SetupSelectableVariant(QuestDifficulty difficulty)
    {
        MockDifficulty(difficulty);

        var variants = GetFilteredVariants(difficulty);
        var selectedVariant = variants[0];
        _usedVariantDescriptions.Add(selectedVariant.Description);

        _randomMock
            .NextItem(
                ArgEx.FluentAssert<IEnumerable<QuestVariant>>(x =>
                    x.Should().BeEquivalentTo(variants)
                )
            )
            .Returns(selectedVariant);

        return selectedVariant;
    }

    private void SetupWinVariant(QuestDifficulty difficulty, int target)
    {
        MockDifficulty(difficulty);

        var variants = GetFilteredVariants(difficulty);

        _randomMock
            .NextItem(
                ArgEx.FluentAssert<IEnumerable<QuestVariant>>(x =>
                    x.Should().BeEquivalentTo(variants)
                )
            )
            .Returns(
                new QuestVariant(
                    Progressor: new WinCondition(),
                    Description: "desc",
                    Target: target,
                    Difficulty: difficulty
                )
            );
    }

    [Fact]
    public async Task GetQuestAsync_returns_a_quest()
    {
        var variant = SetupSelectableVariant(QuestDifficulty.Easy);

        var grain = await CreateGrainAsync();
        var storageStats = GetStorageStats();
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

        storageStats?.Writes.Should().Be(1);
    }

    [Fact]
    public async Task GetQuestAsync_returns_same_quest_if_already_selected_today()
    {
        var variant1 = SetupSelectableVariant(QuestDifficulty.Medium);
        var grain = await CreateGrainAsync();

        var quest1 = await grain.GetQuestAsync();

        var variant2 = SetupSelectableVariant(QuestDifficulty.Easy);
        var quest2 = await grain.GetQuestAsync();

        variant1.Should().NotBeEquivalentTo(variant2);
        quest1.Should().BeEquivalentTo(quest2);
    }

    [Fact]
    public async Task GetQuestAsync_returns_a_new_quest_the_next_day()
    {
        SetupSelectableVariant(QuestDifficulty.Hard);
        var grain = await CreateGrainAsync();
        var quest1 = await grain.GetQuestAsync();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));
        var variant2 = SetupSelectableVariant(QuestDifficulty.Easy);
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
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync();
        var storageStats = GetStorageStats();
        await grain.GetQuestAsync();
        storageStats?.ResetCounts();

        await grain.OnGameOverAsync(snapshot);

        var quest = await grain.GetQuestAsync();
        quest.Progress.Should().Be(1);
        storageStats?.Writes.Should().Be(2);
    }

    [Fact]
    public async Task OnGameOverAsync_does_nothing_if_progressor_returns_zero()
    {
        SetupWinVariant(QuestDifficulty.Easy, target: 2);
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.Black)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync();
        var storageStats = GetStorageStats();
        await grain.OnGameOverAsync(snapshot);

        storageStats?.Writes.Should().Be(0);

        var quest = await grain.GetQuestAsync();
        quest.Progress.Should().Be(0);
    }

    [Fact]
    public async Task OnGameOverAsync_completes_quest_and_updates_state()
    {
        SetupWinVariant(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync();
        var initialQuest = await grain.GetQuestAsync();

        await grain.OnGameOverAsync(snapshot);

        SetupSelectableVariant(QuestDifficulty.Medium);
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
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync();

        // day 1
        await grain.GetQuestAsync();
        await grain.OnGameOverAsync(snapshot);
        var quest1 = await grain.GetQuestAsync();
        quest1.Streak.Should().Be(1);

        // day 2
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));
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
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

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
        SetupSelectableVariant(QuestDifficulty.Easy);
        var grain = await CreateGrainAsync();
        var storageStats = GetStorageStats();
        await grain.GetQuestAsync();
        storageStats?.ResetCounts();

        var variant2 = SetupSelectableVariant(QuestDifficulty.Medium);
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
        storageStats?.Writes.Should().Be(1);

        var questAfter = await grain.GetQuestAsync();
        questAfter.Should().BeEquivalentTo(result.Value);
    }

    [Fact]
    public async Task ReplaceQuestAsync_fails_if_cannot_replace()
    {
        SetupSelectableVariant(QuestDifficulty.Easy);
        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync();

        SetupSelectableVariant(QuestDifficulty.Easy);
        var firstReplacement = await grain.ReplaceQuestAsync();

        SetupSelectableVariant(QuestDifficulty.Hard);
        var secondReplacement = await grain.ReplaceQuestAsync();

        secondReplacement.IsError.Should().BeTrue();
        secondReplacement.FirstError.Should().Be(QuestErrors.CanotReplace);

        var questAfter = await grain.GetQuestAsync();
        questAfter.Should().BeEquivalentTo(firstReplacement.Value);
    }

    [Fact]
    public async Task GetQuestAsync_resets_can_replace()
    {
        SetupSelectableVariant(QuestDifficulty.Easy);
        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync();

        SetupSelectableVariant(QuestDifficulty.Medium);
        var replacement = await grain.ReplaceQuestAsync();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));

        SetupSelectableVariant(QuestDifficulty.Medium);
        var questAfterReset = await grain.GetQuestAsync();
        questAfterReset.Should().NotBe(replacement);
        questAfterReset.CanReplace.Should().BeTrue();
    }

    [Fact]
    public async Task CollectRewardAsync_fails_if_no_reward_pending()
    {
        var grain = await CreateGrainAsync();
        var result = await grain.CollectRewardAsync();
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(QuestErrors.NoRewardToCollect);
    }

    [Fact]
    public async Task CollectRewardAsync_applies_reward_once()
    {
        var user = new AuthedUserFaker().RuleFor(x => x.QuestPoints, 0).Generate();
        await ApiTestBase.DbContext.AddAsync(user, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        SetupWinVariant(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync(user.Id);
        await grain.OnGameOverAsync(snapshot);

        var userAfterGameOver = await _userManager.FindByIdAsync(user.Id);
        userAfterGameOver?.QuestPoints.Should().Be(0);

        var reward = await grain.CollectRewardAsync();
        reward.Value.Should().Be((int)QuestDifficulty.Easy);

        var userAfterClaim = await _userManager.FindByIdAsync(user.Id);
        userAfterClaim?.QuestPoints.Should().Be((int)QuestDifficulty.Easy);

        var secondAttempt = await grain.CollectRewardAsync();
        secondAttempt.IsError.Should().BeTrue();
        secondAttempt.FirstError.Should().Be(QuestErrors.NoRewardToCollect);
    }
}
