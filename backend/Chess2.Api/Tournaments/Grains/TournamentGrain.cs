using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Errors;
using Chess2.Api.Tournaments.Models;
using Chess2.Api.Tournaments.Services;
using Chess2.Api.Tournaments.TournamentFormats;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Tournaments.Grains;

[Alias("Chess2.Api.Tournaments.Grains.ITournamentGrain`1")]
public interface ITournamentGrain<TFormat> : IGrainWithStringKey
    where TFormat : ITournamentFormat
{
    [Alias("CreateTournamentAsync")]
    Task<ErrorOr<Created>> CreateTournamentAsync(
        UserId hostedBy,
        TimeControlSettings timeControl,
        CancellationToken token = default
    );

    [Alias("JoinAsync")]
    Task<ErrorOr<Created>> JoinAsync(UserId userId, CancellationToken token = default);

    [Alias("LeaveAsync")]
    Task<ErrorOr<Deleted>> LeaveAsync(UserId userId, CancellationToken token = default);
}

[GenerateSerializer]
[Alias("Chess2.Api.Tournaments.Grains.Tournament")]
public record TournamentState(UserId HostedBy, TimeControlSettings TimeControl);

[GenerateSerializer]
[Alias("Chess2.Api.Tournaments.Grains.TournamentGrainState")]
public class TournamentGrainState
{
    [Id(0)]
    public TournamentState? Tournament { get; set; }
}

public class TournamentGrain<TFormat> : Grain, IGrainBase, ITournamentGrain<TFormat>
    where TFormat : ITournamentFormat
{
    public const string StateName = "tournament";

    private readonly ILogger<TournamentGrain<TFormat>> _logger;
    private readonly IPersistentState<TournamentGrainState> _state;
    private readonly UserManager<AuthedUser> _userManager;
    private readonly ITournamentService _tournamentService;

    private readonly TournamentToken _token;
    private Dictionary<UserId, TournamentPlayerState> _seekers = [];

    public TournamentGrain(
        ILogger<TournamentGrain<TFormat>> logger,
        [PersistentState(StateName, StorageNames.TournamentState)]
            IPersistentState<TournamentGrainState> state,
        UserManager<AuthedUser> userManager,
        ITournamentService tournamentService
    )
    {
        _logger = logger;
        _state = state;
        _userManager = userManager;
        _tournamentService = tournamentService;

        _token = this.GetPrimaryKeyString();
    }

    public async Task<ErrorOr<Created>> CreateTournamentAsync(
        UserId hostedBy,
        TimeControlSettings timeControl,
        CancellationToken token = default
    )
    {
        if (_state.State.Tournament is not null)
            return TournamentErrors.TournamentAlreadyExists;

        _state.State.Tournament = new TournamentState(hostedBy, timeControl);
        await _state.WriteStateAsync(token);
        await _tournamentService.RegisterTournamentAsync(_token, hostedBy, timeControl, token);

        return Result.Created;
    }

    public async Task<ErrorOr<Created>> JoinAsync(UserId userId, CancellationToken token = default)
    {
        if (_state.State.Tournament is null)
            return TournamentErrors.TournamentNotFound;

        if (!userId.IsAuthed)
            return TournamentErrors.CannotEnterTournament;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning(
                "Cannot find user {UserId} that tried to join tournament {TournamentId}",
                userId,
                _token
            );
            return TournamentErrors.CannotEnterTournament;
        }

        var seeker = await _tournamentService.AddPlayerAsync(
            user,
            _token,
            _state.State.Tournament.TimeControl,
            token
        );
        _seekers[user.Id] = seeker;

        return Result.Created;
    }

    public async Task<ErrorOr<Deleted>> LeaveAsync(UserId userId, CancellationToken token = default)
    {
        if (_state.State.Tournament is null)
            return TournamentErrors.TournamentNotFound;

        if (!_seekers.ContainsKey(userId))
            return TournamentErrors.NotPartOfTournament;

        await _tournamentService.RemovePlayerAsync(userId, _token, token);
        _seekers.Remove(userId);
        return Result.Deleted;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (_state.State.Tournament is not null)
        {
            _seekers = await _tournamentService.GetTournamentPlayers(
                _token,
                _state.State.Tournament.TimeControl,
                cancellationToken
            );
        }

        await base.OnActivateAsync(cancellationToken);
    }
}
