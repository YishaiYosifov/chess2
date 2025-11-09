using Chess2.Api.Matchmaking.Services.Pools;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Models;
using Chess2.Api.Tournaments.Models;
using Chess2.Api.Tournaments.Services;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Tournaments.Grains;

[Alias("Chess2.Api.Tournaments.Grains.IArenaTournamentGrain")]
public interface IArenaTournamentFormatGrain : ITournamentFormatGrain;

public class ArenaTournamentFormatGrain(
    IOptions<AppSettings> settings,
    ITournamentPlayerService tournamentPlayerService
) : Grain, IGrainBase, IArenaTournamentFormatGrain
{
    private readonly TournamentSettings _settings = settings.Value.Tournament;
    private readonly ITournamentPlayerService _tournamentPlayerService = tournamentPlayerService;

    private readonly RatedMatchmakingPool _pool = new();
    private IGrainTimer? _waveTimer;

    public async Task StartAsync(CancellationToken token = default)
    {
        await CreateMatchesAsync(token);
        _waveTimer = this.RegisterGrainTimer(
            callback: CreateMatchesAsync,
            dueTime: _settings.ArenaWaveEvery,
            period: _settings.ArenaWaveEvery
        );
    }

    public Task PlayerAvailableAsync(
        TournamentPlayerState player,
        CancellationToken token = default
    )
    {
        _pool.AddSeeker(player.Seeker);
        return Task.CompletedTask;
    }

    public Task PlayerUnavailableAsync(UserId userId, CancellationToken token = default)
    {
        _pool.RemoveSeeker(userId);
        return Task.CompletedTask;
    }

    private async Task CreateMatchesAsync(CancellationToken token = default)
    {
        foreach (var (userId1, userId2) in _pool.CalculateMatches())
        {
            await _tournamentPlayerService.MatchPlayersAsync(userId1, userId2, token);
            _pool.RemoveSeeker(userId1);
            _pool.RemoveSeeker(userId2);
        }
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await SyncPlayersAsync();
        await base.OnActivateAsync(cancellationToken);
    }

    private async Task SyncPlayersAsync()
    {
        var tournamentGrain = GrainFactory.GetGrain<ITournamentGrain>(this.GetPrimaryKeyString());
        var players = await tournamentGrain.GetPlayerAsync();
        foreach (var player in players.Values)
        {
            if (player.InGame is null)
                _pool.AddSeeker(player.Seeker);
        }
    }
}
