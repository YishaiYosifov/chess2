using Chess2.Api.Quests.Grains;
using Chess2.Api.Quests.Services;
using Chess2.Api.TestInfrastructure;
using Chess2.Api.TestInfrastructure.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.QuestTests;

public class QuestSeasonResetterGrainTests : BaseOrleansIntegrationTest
{
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly DateTimeOffset _fakeNow = DateTimeOffset.UtcNow;

    public QuestSeasonResetterGrainTests(Chess2WebApplicationFactory factory)
        : base(factory)
    {
        _timeProviderMock.GetUtcNow().Returns(_fakeNow);

        Silo.ServiceProvider.AddService(_timeProviderMock);
        Silo.ServiceProvider.AddService(
            ApiTestBase.Scope.ServiceProvider.GetRequiredService<IQuestService>()
        );
    }

    [Fact]
    public async Task InitializeAsync_registers_a_reminder()
    {
        var grain = await Silo.CreateGrainAsync<QuestSeasonResetterGrain>(0);

        using (await Silo.GetReminderActivationContext(grain, ApiTestBase.CT))
        {
            await grain.InitializeAsync();
        }

        var startOfNextMonth = new DateTime(_fakeNow.Year, _fakeNow.Month, 1).AddMonths(1);
        var dueTime = startOfNextMonth - _fakeNow;
        Silo.ReminderRegistry.Mock.Verify(x =>
            x.RegisterOrUpdateReminder(
                Silo.GetGrainId(grain),
                QuestSeasonResetterGrain.ReminderName,
                dueTime,
                TimeSpan.FromDays(30)
            )
        );
    }

    [Fact]
    public async Task InitializeAsync_doesnt_reregister_reminder_if_it_already_exists()
    {
        var grain = await Silo.CreateGrainAsync<QuestSeasonResetterGrain>(0);

        using (await Silo.GetReminderActivationContext(grain, ApiTestBase.CT))
        {
            await grain.InitializeAsync();
        }
        Silo.ReminderRegistry.Mock.Reset();

        using (await Silo.GetReminderActivationContext(grain, ApiTestBase.CT))
        {
            await grain.InitializeAsync();
        }

        Silo.ReminderRegistry.Mock.Verify(
            x =>
                x.RegisterOrUpdateReminder(
                    Silo.GetGrainId(grain),
                    It.IsAny<string>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<TimeSpan>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ReceiveReminder_resets_all_quest_points_and_registers_reminder()
    {
        var questPoints = new UserQuestPointsFaker().Generate(5);
        await ApiTestBase.DbContext.AddRangeAsync(questPoints, ApiTestBase.CT);
        await ApiTestBase.DbContext.SaveChangesAsync(ApiTestBase.CT);

        var grain = await Silo.CreateGrainAsync<QuestSeasonResetterGrain>(0);

        using (await Silo.GetReminderActivationContext(grain, ApiTestBase.CT))
        {
            await grain.ReceiveReminder(QuestSeasonResetterGrain.ReminderName, new TickStatus());
        }

        (await ApiTestBase.DbContext.QuestPoints.ToListAsync(ApiTestBase.CT)).Should().BeEmpty();
        Silo.ReminderRegistry.Mock.Verify(x =>
            x.RegisterOrUpdateReminder(
                Silo.GetGrainId(grain),
                QuestSeasonResetterGrain.ReminderName,
                It.IsAny<TimeSpan>(),
                TimeSpan.FromDays(30)
            )
        );
    }
}
