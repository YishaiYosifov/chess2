using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Quests.DTOs;
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

    private QuestVariant MockVariant(QuestDifficulty difficulty, int index = 0)
    {
        MockDifficulty(difficulty);

        var variants = _quests
            .SelectMany(quest => quest.Variants.Where(variant => variant.Difficulty == difficulty))
            .ToList();
        var selectedVariant = variants[index];
        _randomMock
            .NextItem(
                ArgEx.FluentAssert<IEnumerable<QuestVariant>>(x =>
                    x.Should().BeEquivalentTo(variants)
                )
            )
            .Returns(selectedVariant);
        return selectedVariant;
    }

    private void MockWinQuest(QuestDifficulty difficulty, int target)
    {
        MockDifficulty(difficulty);

        var variants = _quests
            .SelectMany(quest => quest.Variants.Where(variant => variant.Difficulty == difficulty))
            .ToList();
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
        var variant = MockVariant(QuestDifficulty.Easy);

        var grain = await CreateGrainAsync();
        var storageStats = GetStorageStats();
        var quest = await grain.GetQuestAsync();

        quest
            .Should()
            .BeEquivalentTo(
                new QuestDto(
                    QuestDifficulty.Easy,
                    variant.Description,
                    Progress: 0,
                    Target: variant.Target,
                    Streak: 0
                )
            );

        storageStats?.Writes.Should().Be(1);
    }

    [Fact]
    public async Task GetQuestAsync_returns_same_quest_if_already_selected_today()
    {
        var variant1 = MockVariant(QuestDifficulty.Medium, index: 0);
        var grain = await CreateGrainAsync();

        var storageStats = GetStorageStats();
        var quest1 = await grain.GetQuestAsync();
        storageStats?.ResetCounts();

        var variant2 = MockVariant(QuestDifficulty.Easy, index: 1);
        var quest2 = await grain.GetQuestAsync();

        variant1.Should().NotBeEquivalentTo(variant2);
        quest1.Should().BeEquivalentTo(quest2);
        storageStats?.Writes.Should().Be(0);
    }

    [Fact]
    public async Task GetQuestAsync_returns_a_new_quest_the_next_day()
    {
        MockVariant(QuestDifficulty.Hard, index: 0);
        var grain = await CreateGrainAsync();
        var quest1 = await grain.GetQuestAsync();

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));
        var variant2 = MockVariant(QuestDifficulty.Easy, index: 1);
        var quest2 = await grain.GetQuestAsync();

        quest1.Should().NotBeEquivalentTo(quest2);
        quest2
            .Should()
            .BeEquivalentTo(
                new QuestDto(
                    variant2.Difficulty,
                    variant2.Description,
                    Progress: 0,
                    Target: variant2.Target,
                    Streak: 0
                )
            );
    }

    [Fact]
    public async Task OnGameOverAsync_increments_progress()
    {
        MockWinQuest(QuestDifficulty.Easy, target: 2);
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
        storageStats?.Writes.Should().Be(1);
    }

    [Fact]
    public async Task OnGameOverAsync_does_nothing_if_progressor_returns_zero()
    {
        MockWinQuest(QuestDifficulty.Easy, target: 2);
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.Black)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync();
        var storageStats = GetStorageStats();
        await grain.OnGameOverAsync(snapshot);

        storageStats?.Writes.Should().Be(1); // 1 write for creating the quest

        var quest = await grain.GetQuestAsync();
        quest.Progress.Should().Be(0);
    }

    [Fact]
    public async Task OnGameOverAsync_completes_quest_and_awards_points()
    {
        var user = new AuthedUserFaker().RuleFor(x => x.QuestPoints, 0).Generate();
        await ApiTestBase.DbContext.AddAsync(user, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        MockWinQuest(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync(user.Id);
        await grain.GetQuestAsync();

        await grain.OnGameOverAsync(snapshot);

        var updatedUser = await _userManager.FindByIdAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.QuestPoints.Should().Be((int)QuestDifficulty.Easy);
    }

    [Fact]
    public async Task OnGameOverAsync_increments_streak_across_multiple_days()
    {
        var user = new AuthedUserFaker().Generate();
        await ApiTestBase.DbContext.AddAsync(user, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        MockWinQuest(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync(user.Id);

        // day 1
        await grain.GetQuestAsync();
        await grain.OnGameOverAsync(snapshot);
        var quest1 = await grain.GetQuestAsync();
        quest1.Streak.Should().Be(1);

        // day 2
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));
        MockWinQuest(QuestDifficulty.Easy, target: 1);
        await grain.GetQuestAsync();
        await grain.OnGameOverAsync(snapshot);
        var quest2 = await grain.GetQuestAsync();
        quest2.Streak.Should().Be(2);
    }

    [Fact]
    public async Task Streak_resets_if_a_day_is_missed()
    {
        var user = new AuthedUserFaker().Generate();
        await ApiTestBase.DbContext.AddAsync(user, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        MockWinQuest(QuestDifficulty.Easy, target: 1);
        var snapshot = new GameQuestSnapshotFaker()
            .RuleFor(x => x.PlayerColor, GameColor.White)
            .RuleFor(x => x.ResultData, new GameResultDataFaker(GameResult.WhiteWin))
            .Generate();

        var grain = await CreateGrainAsync(user.Id);

        // day 1 complete quest
        await grain.GetQuestAsync();
        await grain.OnGameOverAsync(snapshot);
        (await grain.GetQuestAsync()).Streak.Should().Be(1);

        // skip a day
        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(2));
        MockWinQuest(QuestDifficulty.Easy, target: 1);
        var questAfterSkip = await grain.GetQuestAsync();

        questAfterSkip.Streak.Should().Be(0);
    }
}
