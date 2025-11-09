using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Models;

namespace Chess2.Api.Tournaments.Grains;

[Alias("Chess2.Api.Tournaments.Grains.IFormatGrain")]
public interface ITournamentFormatGrain : IGrainWithStringKey
{
    [Alias("StartAsync")]
    Task StartAsync(CancellationToken token = default);

    [Alias("AddPlayer")]
    Task PlayerAvailableAsync(TournamentPlayerState player, CancellationToken token = default);

    [Alias("RemovePlayer")]
    Task PlayerUnavailableAsync(UserId userId, CancellationToken token = default);
}
