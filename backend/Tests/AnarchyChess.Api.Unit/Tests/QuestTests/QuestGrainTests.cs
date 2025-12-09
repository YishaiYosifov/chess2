using AnarchyChess.Api.Game.Grains;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.QuestLogic.QuestConditions;
using AnarchyChess.Api.QuestLogic.QuestMetrics;
using AnarchyChess.Api.Quests.DTOs;
using AnarchyChess.Api.Quests.Errors;
using AnarchyChess.Api.Quests.Grains;
using AnarchyChess.Api.Quests.Services;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.Api.TestInfrastructure.NSubtituteExtenstion;
using AwesomeAssertions;
using NSubstitute;
using Orleans.TestKit;
using Orleans.TestKit.Storage;
using Orleans.TestKit.Streams;

namespace AnarchyChess.Api.Unit.Tests.QuestTests;

public class QuestGrainTests : BaseGrainTest
{
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();
    private readonly IRandomQuestProvider _randomQuestProviderMock =
        Substitute.For<IRandomQuestProvider>();
    private readonly IGameGrain _gameGrainMock = Substitute.For<IGameGrain>();

    private readonly TestStorageStats _stateStats;

    private readonly GameToken _testGameToken = "test game token";
    private readonly AuthedUser _testUser;
    private readonly GameState _testGameState;
    private readonly List<Move> _testMoves = [.. new MoveFaker().Generate(5)];
    private DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;
    private QuestInstance? _lastInstance;

    public QuestGrainTests()
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        _testUser = new AuthedUserFaker().Generate();
        _testGameState = new GameStateFaker().RuleFor(
            x => x.WhitePlayer,
            new GamePlayerFaker(GameColor.White).RuleFor(x => x.UserId, _testUser.Id).Generate()
        );
        _gameGrainMock.GetStateAsync().Returns(_testGameState);
        _gameGrainMock.GetMovesAsync().Returns(_testMoves);
        Silo.AddProbe(id =>
            id.ToString() == _testGameToken ? _gameGrainMock : Substitute.For<IGameGrain>()
        );

        Silo.ServiceProvider.AddService(_randomQuestProviderMock);
        Silo.ServiceProvider.AddService(_timeProviderMock);

        Silo.StorageManager.GetStorage<QuestGrainStorage>(QuestGrain.StateName);
        _stateStats = Silo.StorageManager.GetStorageStats(QuestGrain.StateName)!;
    }

    private TestStream<GameEndedEvent> ProbeGameEndedStream() =>
        Silo.AddStreamProbe<GameEndedEvent>(
            _testUser.Id,
            streamNamespace: nameof(GameEndedEvent),
            Streaming.StreamProvider
        );

    private async Task<IQuestGrain> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<QuestGrain>(_testUser.Id);

    private QuestVariant SetupWinQuestVariant(int? target = null, DateTimeOffset? date = null) =>
        SetupQuestVariant([new WinCondition()], target: target, date: date);

    private QuestVariant SetupQuestVariant(
        List<IQuestCondition> conditions,
        List<IQuestMetric>? progressors = null,
        int? target = null,
        DateTimeOffset? date = null
    )
    {
        var variant = new QuestVariantFaker(conditions, progressors, target).Generate();
        var instance = variant.CreateInstance(DateOnly.FromDateTime((date ?? _fakeNow).DateTime));

        var previousInstance = _lastInstance;
        _randomQuestProviderMock
            .GetRandomQuestInstance(
                ArgEx.FluentAssert<QuestInstance?>(x => x.Should().BeEquivalentTo(previousInstance))
            )
            .Returns(instance);
        _lastInstance = instance;

        return variant;
    }

    [Fact]
    public async Task GetQuestAsync_returns_a_quest()
    {
        var variant = SetupWinQuestVariant();

        var grain = await CreateGrainAsync();
        var quest = await grain.GetQuestAsync(CT);

        quest
            .Should()
            .BeEquivalentTo(
                new QuestDto(
                    variant.Difficulty,
                    variant.Description,
                    Target: variant.Target,
                    Progress: 0,
                    CanReplace: true,
                    Streak: 0,
                    RewardCollected: false
                )
            );

        _stateStats.Writes.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetQuestAsync_returns_same_quest_if_already_selected_today()
    {
        var variant1 = SetupWinQuestVariant();
        var grain = await CreateGrainAsync();

        var quest1 = await grain.GetQuestAsync(CT);

        var variant2 = SetupWinQuestVariant();
        var quest2 = await grain.GetQuestAsync(CT);

        variant1.Should().NotBeEquivalentTo(variant2);
        quest1.Should().BeEquivalentTo(quest2);
    }

    [Fact]
    public async Task GetQuestAsync_returns_a_new_quest_the_next_day()
    {
        SetupWinQuestVariant();
        var grain = await CreateGrainAsync();
        var quest1 = await grain.GetQuestAsync(CT);

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));
        var variant2 = SetupWinQuestVariant();
        var quest2 = await grain.GetQuestAsync(CT);

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
    public async Task GameEndedEvent_increments_progress()
    {
        SetupWinQuestVariant(target: 2);

        var gameOverStream = ProbeGameEndedStream();
        var grain = await CreateGrainAsync();

        await gameOverStream.OnNextAsync(
            new GameEndedEvent(
                _testGameToken,
                new GameResultDataFaker(GameResult.WhiteWin).Generate()
            )
        );

        var quest = await grain.GetQuestAsync(CT);
        quest.Progress.Should().Be(1);
        _stateStats.Writes.Should().Be(2);
    }

    [Fact]
    public async Task GameEndedEvent_does_nothing_if_conditions_not_met()
    {
        SetupWinQuestVariant();

        var gameOverStream = ProbeGameEndedStream();
        var grain = await CreateGrainAsync();

        await gameOverStream.OnNextAsync(
            new GameEndedEvent(
                _testGameToken,
                new GameResultDataFaker(GameResult.BlackWin).Generate()
            )
        );

        var quest = await grain.GetQuestAsync(CT);
        quest.Progress.Should().Be(0);
    }

    [Fact]
    public async Task GameEndedEvent_completes_quest_and_updates_state()
    {
        SetupWinQuestVariant(target: 1);

        var gameOverStream = ProbeGameEndedStream();
        var grain = await CreateGrainAsync();
        var initialQuest = await grain.GetQuestAsync(CT);

        await gameOverStream.OnNextAsync(
            new GameEndedEvent(
                _testGameToken,
                new GameResultDataFaker(GameResult.WhiteWin).Generate()
            )
        );

        SetupWinQuestVariant();
        var questAfterCompletion = await grain.GetQuestAsync(CT);
        questAfterCompletion.Description.Should().Be(initialQuest.Description);
        questAfterCompletion.CanReplace.Should().BeFalse();
        questAfterCompletion.RewardCollected.Should().BeFalse();

        // quest replacement should still fail until a new quest is selected
        var replaceAttempt = await grain.ReplaceQuestAsync(CT);
        replaceAttempt.IsError.Should().BeTrue();
        replaceAttempt.FirstError.Should().Be(QuestErrors.CanotReplace);
    }

    [Fact]
    public async Task GameEndedEvent_ignores_aborted_games()
    {
        SetupWinQuestVariant(target: 1);
        var gameOverStream = ProbeGameEndedStream();
        var grain = await CreateGrainAsync();

        await gameOverStream.OnNextAsync(
            new GameEndedEvent(
                _testGameToken,
                new GameResultDataFaker(GameResult.Aborted).Generate()
            )
        );

        var quest = await grain.GetQuestAsync(CT);
        quest.Progress.Should().Be(0);
    }

    [Fact]
    public async Task GameEndedEvent_creates_correct_GameQuestSnapshot()
    {
        var conditionMock = Substitute.For<IQuestCondition>();
        conditionMock.Evaluate(Arg.Any<GameQuestSnapshot>()).Returns(true);
        SetupQuestVariant([conditionMock]);

        var gameOverStream = ProbeGameEndedStream();
        await CreateGrainAsync();

        var result = new GameResultDataFaker(GameResult.WhiteWin).Generate();
        await gameOverStream.OnNextAsync(new GameEndedEvent(_testGameToken, result));

        GameQuestSnapshot expectedSnapshot = new(
            GameColor.White,
            _testMoves,
            result,
            _testGameState
        );
        conditionMock
            .Received(1)
            .Evaluate(
                ArgEx.FluentAssert<GameQuestSnapshot>(x =>
                    x.Should().BeEquivalentTo(expectedSnapshot)
                )
            );
    }

    [Fact]
    public async Task Streak_resets_if_a_day_is_missed()
    {
        SetupWinQuestVariant(target: 1);

        var gameOverStream = ProbeGameEndedStream();
        var grain = await CreateGrainAsync();

        // day 1 complete quest
        await gameOverStream.OnNextAsync(
            new GameEndedEvent(
                _testGameToken,
                new GameResultDataFaker(GameResult.WhiteWin).Generate()
            )
        );
        await grain.CollectRewardAsync(CT);
        (await grain.GetQuestAsync(CT)).Streak.Should().Be(1);

        // skip a day
        _fakeNow += TimeSpan.FromDays(2);
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        SetupWinQuestVariant(target: 1);
        var questAfterSkip = await grain.GetQuestAsync(CT);

        questAfterSkip.Streak.Should().Be(0);
    }

    [Fact]
    public async Task ReplaceQuestAsync_replaces_quest_when_allowed()
    {
        SetupWinQuestVariant();
        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync(CT);
        _stateStats.ResetCounts();

        var variant2 = SetupWinQuestVariant();
        var result = await grain.ReplaceQuestAsync(CT);

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

        var questAfter = await grain.GetQuestAsync(CT);
        questAfter.Should().BeEquivalentTo(result.Value);
    }

    [Fact]
    public async Task ReplaceQuestAsync_fails_if_cannot_replace()
    {
        SetupWinQuestVariant();
        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync(CT);

        SetupWinQuestVariant();
        var firstReplacement = await grain.ReplaceQuestAsync(CT);

        SetupWinQuestVariant();
        var secondReplacement = await grain.ReplaceQuestAsync(CT);

        secondReplacement.IsError.Should().BeTrue();
        secondReplacement.FirstError.Should().Be(QuestErrors.CanotReplace);

        var questAfter = await grain.GetQuestAsync(CT);
        questAfter.Should().BeEquivalentTo(firstReplacement.Value);
    }

    [Fact]
    public async Task GetQuestAsync_resets_can_replace()
    {
        SetupWinQuestVariant();
        var grain = await CreateGrainAsync();
        await grain.GetQuestAsync(CT);

        SetupWinQuestVariant();
        var replacement = await grain.ReplaceQuestAsync(CT);

        _timeProviderMock.GetUtcNow().Returns(_fakeNow + TimeSpan.FromDays(1));

        SetupWinQuestVariant();
        var questAfterReset = await grain.GetQuestAsync(CT);
        questAfterReset.Should().NotBe(replacement);
        questAfterReset.CanReplace.Should().BeTrue();
    }

    [Fact]
    public async Task CollectRewardAsync_increments_streak_across_multiple_days()
    {
        SetupWinQuestVariant(target: 1);

        var gameOverStream = ProbeGameEndedStream();
        var grain = await CreateGrainAsync();

        // day 1
        await gameOverStream.OnNextAsync(
            new GameEndedEvent(
                _testGameToken,
                new GameResultDataFaker(GameResult.WhiteWin).Generate()
            )
        );
        await grain.CollectRewardAsync(CT);
        var quest1 = await grain.GetQuestAsync(CT);
        quest1.Streak.Should().Be(1);

        // day 2
        _fakeNow += TimeSpan.FromDays(1);
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);
        SetupWinQuestVariant(target: 1);
        await gameOverStream.OnNextAsync(
            new GameEndedEvent(
                _testGameToken,
                new GameResultDataFaker(GameResult.WhiteWin).Generate()
            )
        );
        await grain.CollectRewardAsync(CT);
        var quest2 = await grain.GetQuestAsync(CT);
        quest2.Streak.Should().Be(2);
    }

    [Fact]
    public async Task CollectRewardAsync_fails_if_no_reward_pending()
    {
        SetupWinQuestVariant();
        var grain = await CreateGrainAsync();

        var result = await grain.CollectRewardAsync(CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(QuestErrors.NoRewardToCollect);
    }
}
