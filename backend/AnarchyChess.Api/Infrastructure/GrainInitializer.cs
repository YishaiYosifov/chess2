using AnarchyChess.Api.Lobby.Grains;
using AnarchyChess.Api.Quests.Grains;
using AnarchyChess.Api.Shared.Models;
using Microsoft.Extensions.Options;

namespace AnarchyChess.Api.Infrastructure;

public class GrainInitializer(
    ILogger<GrainInitializer> logger,
    IGrainFactory grains,
    IOptions<AppSettings> settings
) : IHostedService
{
    private readonly ILogger<GrainInitializer> _logger = logger;
    private readonly IGrainFactory _grains = grains;
    private readonly AppSettings _settings = settings.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await InitializeGrains();
                return;
            }
            catch (OrleansException ex)
            {
                _logger.LogError(ex, "Failed to initialize grains, trying again in 2 seconds");
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    private async Task InitializeGrains()
    {
        var grain = _grains.GetGrain<IQuestSeasonResetterGrain>(0);
        await grain.InitializeAsync();

        for (var i = 0; i < _settings.Lobby.OpenSeekShardCount; i++)
        {
            await _grains.GetGrain<IOpenSeekGrain>(i).InitializeAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
