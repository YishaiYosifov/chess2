using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Matchmaking.Services;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Tournaments.Errors;
using Chess2.Api.Tournaments.Models;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Tournaments.Grains;

[Alias("Chess2.Api.Tournaments.Grains.ITournamentPlayerGrain")]
public interface ITournamentPlayerGrain : IGrainWithStringKey
{
    [Alias("CreateAsync")]
    Task<ErrorOr<Created>> CreateAsync(
        TimeControlSettings timeControl,
        CancellationToken token = default
    );
}

[GenerateSerializer]
[Alias("Chess2.Api.Tournaments.Grains.TournamentPlayerGrainState")]
public class TournamentPlayerGrainState
{
    [Id(0)]
    public Seeker? PlayerSeeker { get; set; }
}

public class TournamentPlayerGrain : Grain, IGrainBase, ITournamentPlayerGrain
{
    public const string StateName = "tournamentPlayer";

    private readonly ILogger<TournamentPlayerGrain> _logger;
    private readonly IPersistentState<TournamentPlayerGrainState> _state;
    private readonly ISeekerCreator _seekerCreator;
    private readonly UserManager<AuthedUser> _userManager;

    private readonly TournamentPlayerGrainKey _key;

    public TournamentPlayerGrain(
        ILogger<TournamentPlayerGrain> logger,
        [PersistentState(StateName, StorageNames.TournamentPlayerState)]
            IPersistentState<TournamentPlayerGrainState> state,
        ISeekerCreator seekerCreator,
        UserManager<AuthedUser> userManager
    )
    {
        _key = TournamentPlayerGrainKey.FromKey(this.GetPrimaryKeyString());

        _logger = logger;
        _state = state;
        _seekerCreator = seekerCreator;
        _userManager = userManager;
    }

    public async Task<ErrorOr<Created>> CreateAsync(
        TimeControlSettings timeControl,
        CancellationToken token = default
    )
    {
        if (_state.State.PlayerSeeker is not null)
            return Result.Created;

        var user = await _userManager.FindByIdAsync(_key.UserId);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found when joining tournament", _key.UserId);
            return TournamentErrors.CannotEnterTournament;
        }

        var seeker = await _seekerCreator.CreateRatedSeekerAsync(user, timeControl, token);
        _state.State.PlayerSeeker = seeker;
        await _state.WriteStateAsync(token);

        return Result.Created;
    }
}
