using Chess2.Api.Infrastructure;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Models;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Tournaments.Grains;

[Alias("Chess2.Api.Tournaments.Grains.ITournamentPlayerGrain")]
public interface ITournamentPlayerGrain : IGrainWithIntegerCompoundKey
{
    [Alias("CreateAsync")]
    Task<ErrorOr<Created>> CreateAsync(CancellationToken token = default);
}

[GenerateSerializer]
[Alias("Chess2.Api.Tournaments.Grains.TournamentPlayer")]
public record TournamentPlayer(UserId UserId, TournamentId TournamentId, Seeker Seeker);

[GenerateSerializer]
[Alias("Chess2.Api.Tournaments.Grains.TournamentPlayerGrainState")]
public class TournamentPlayerGrainState
{
    [Id(0)]
    public TournamentPlayer? Player { get; set; }
}

public class TournamentPlayerGrain(
    [PersistentState(TournamentPlayerGrain.StateName, StorageNames.TournamentPlayerState)]
        IPersistentState<TournamentPlayerGrainState> state,
    ISeekerCreator seekerCreator,
    UserManager<AuthedUser> userManager
) : Grain, IGrainBase, ITournamentPlayerGrain
{
    public const string StateName = "tournamentPlayer";

    private readonly IPersistentState<TournamentPlayerGrainState> _state = state;
    private readonly ISeekerCreator _seekerCreator = seekerCreator;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    public Task<ErrorOr<Created>> CreateAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
