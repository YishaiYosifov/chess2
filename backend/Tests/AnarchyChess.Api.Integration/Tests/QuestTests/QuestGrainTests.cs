using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.QuestLogic;
using AnarchyChess.Api.QuestLogic.Models;
using AnarchyChess.Api.Quests.Errors;
using AnarchyChess.Api.Quests.Grains;
using AnarchyChess.Api.Quests.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Core;

namespace AnarchyChess.Api.Integration.Tests.QuestTests;

public class QuestGrainTests : BaseOrleansIntegrationTest
{
    private readonly IQuestService _questService;

    private readonly IStorage<QuestGrainStorage> _storage;

    private readonly AuthedUser _testUser;

    public QuestGrainTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _questService = ApiTestBase.Scope.ServiceProvider.GetRequiredService<IQuestService>();
        _testUser = new AuthedUserFaker().Generate();

        Silo.ServiceProvider.AddService(_questService);
        Silo.ServiceProvider.AddService(
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IRandomQuestProvider>()
        );
        Silo.ServiceProvider.AddService(
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<TimeProvider>()
        );

        _storage = Silo.StorageManager.GetStorage<QuestGrainStorage>(QuestGrain.StateName);
    }

    private async Task<IQuestGrain> CreateGrainAsync() =>
        await Silo.CreateGrainAsync<QuestGrain>(_testUser.Id);

    [Fact]
    public async Task CollectRewardAsync_applies_reward_once()
    {
        var questPoints = new UserQuestPointsFaker(_testUser).Generate();
        await ApiTestBase.DbContext.AddAsync(questPoints, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        var grain = await CreateGrainAsync();
        _storage.State.Quest = new QuestInstance(
            description: "test",
            QuestDifficulty.Easy,
            target: 1,
            DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime),
            shouldResetOnFailure: false,
            conditions: [],
            metrics: null
        );
        // no metrics, should complete
        _storage.State.Quest.ApplySnapshot(new GameQuestSnapshotFaker().Generate());

        var pointsGameOver = await _questService.GetQuestPointsAsync(
            questPoints.UserId,
            ApiTestBase.CT
        );
        pointsGameOver.Should().Be(0);

        var reward = await grain.CollectRewardAsync(ApiTestBase.CT);
        reward.Value.Should().Be((int)QuestDifficulty.Easy);

        var pointsAfterClaim = await _questService.GetQuestPointsAsync(
            questPoints.UserId,
            ApiTestBase.CT
        );
        pointsAfterClaim.Should().Be((int)QuestDifficulty.Easy);

        var secondAttempt = await grain.CollectRewardAsync(ApiTestBase.CT);
        secondAttempt.IsError.Should().BeTrue();
        secondAttempt.FirstError.Should().Be(QuestErrors.NoRewardToCollect);
    }
}
