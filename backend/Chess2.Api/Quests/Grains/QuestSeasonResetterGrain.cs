using Chess2.Api.Quests.Services;

namespace Chess2.Api.Quests.Grains;

[Alias("Chess2.Api.Quests.Grains.IQuestSeasonResetterGrain")]
public interface IQuestSeasonResetterGrain : IGrainWithIntegerKey
{
    [Alias("InitializeAsync")]
    Task InitializeAsync();
}

public class QuestSeasonResetterGrain(
    IQuestService questService,
    ILogger<QuestSeasonResetterGrain> logger
) : Grain, IQuestSeasonResetterGrain, IRemindable
{
    private const string ReminderName = "QuestSeasonResetterReminder";
    private readonly IQuestService _questService = questService;
    private readonly ILogger<QuestSeasonResetterGrain> _logger = logger;

    public async Task InitializeAsync()
    {
        var reminder = await this.GetReminder(ReminderName);
        if (reminder is not null)
            return;

        await SetupReminder();
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        _logger.LogInformation("Resetting ALL quest points for new season");
        await _questService.ResetAllQuestPointsAsync();
        await SetupReminder();
    }

    private async Task SetupReminder()
    {
        var now = DateTime.UtcNow;
        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        var dueTime = nextMonth - now;

        await this.RegisterOrUpdateReminder(ReminderName, dueTime, period: TimeSpan.MaxValue);
    }
}
