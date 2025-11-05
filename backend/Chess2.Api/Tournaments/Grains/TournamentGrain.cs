using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Errors;
using Chess2.Api.Tournaments.Repositories;
using Chess2.Api.Tournaments.Services;
using Chess2.Api.Tournaments.TournamentFormats;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Tournaments.Grains;

[Alias("Chess2.Api.Tournaments.Grains.ITournamentGrain`1")]
public interface ITournamentGrain<TFormat> : IGrainWithStringKey
    where TFormat : ITournamentFormat
{
    [Alias("CreateAsync")]
    Task<ErrorOr<Created>> CreateAsync(
        UserId hostedBy,
        TimeControlSettings timeControl,
        CancellationToken token = default
    );

    [Alias("JoinAsync")]
    Task<ErrorOr<Created>> JoinAsync(UserId userId, CancellationToken token = default);

    [Alias("LeaveAsync")]
    Task<ErrorOr<Deleted>> LeaveAsync(UserId userId, CancellationToken token = default);

    [Alias("StartAsync")]
    Task<ErrorOr<Success>> StartAsync(UserId startedBy, CancellationToken token = default);
}

public class TournamentGrain<TFormat>(
    ILogger<TournamentGrain<TFormat>> logger,
    UserManager<AuthedUser> userManager,
    ITournamentPlayerService tournamentPlayerService,
    ITournamentRepository tournamentRepository,
    IUnitOfWork unitOfWork
) : Grain, IGrainBase, ITournamentGrain<TFormat>
    where TFormat : ITournamentFormat, new()
{
    public const string StateName = "tournament";

    private readonly ILogger<TournamentGrain<TFormat>> _logger = logger;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly ITournamentPlayerService _tournamentPlayerService = tournamentPlayerService;
    private readonly ITournamentRepository _tournamentRepository = tournamentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly ITournamentFormat _format = new TFormat();

    private Tournament? _tournament;

    public async Task<ErrorOr<Created>> CreateAsync(
        UserId hostedBy,
        TimeControlSettings timeControl,
        CancellationToken token = default
    )
    {
        if (_tournament is not null)
            return TournamentErrors.TournamentAlreadyExists;

        _tournament = new()
        {
            TournamentToken = this.GetPrimaryKeyString(),
            HostedBy = hostedBy,
            BaseSeconds = timeControl.BaseSeconds,
            IncrementSeconds = timeControl.IncrementSeconds,
            Format = _format.Format,
        };
        await _tournamentRepository.AddTournamentAsync(_tournament, token);
        await _unitOfWork.CompleteAsync(token);

        return Result.Created;
    }

    public async Task<ErrorOr<Success>> StartAsync(
        UserId startedBy,
        CancellationToken token = default
    )
    {
        if (_tournament is null)
            return TournamentErrors.TournamentNotFound;
        if (startedBy != _tournament.HostedBy)
            return TournamentErrors.NoHostPermissions;

        _tournament.HasStarted = true;
        _tournamentRepository.UpdateTournament(_tournament);
        await _unitOfWork.CompleteAsync(token);

        return Result.Success;
    }

    public async Task<ErrorOr<Created>> JoinAsync(UserId userId, CancellationToken token = default)
    {
        if (_tournament is null)
            return TournamentErrors.TournamentNotFound;

        if (!userId.IsAuthed)
            return TournamentErrors.CannotEnterTournament;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning(
                "Cannot find user {UserId} that tried to join tournament {TournamentId}",
                userId,
                _tournament.TournamentToken
            );
            return TournamentErrors.CannotEnterTournament;
        }

        var player = await _tournamentPlayerService.AddPlayerAsync(user, _tournament, token);
        _format.AddPlayer(player);

        return Result.Created;
    }

    public async Task<ErrorOr<Deleted>> LeaveAsync(UserId userId, CancellationToken token = default)
    {
        if (_tournament is null)
            return TournamentErrors.TournamentNotFound;

        if (!_format.HasPlayer(userId))
            return TournamentErrors.NotPartOfTournament;

        await _tournamentPlayerService.RemovePlayerAsync(
            userId,
            _tournament.TournamentToken,
            token
        );
        _format.RemovePlayer(userId);

        return Result.Deleted;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _tournament = await _tournamentRepository.GetByTokenAsync(
            tournamentToken: this.GetPrimaryKeyString(),
            cancellationToken
        );

        if (_tournament is not null)
        {
            var players = _tournamentPlayerService.GetTournamentPlayersAsync(
                _tournament,
                cancellationToken
            );
            await foreach (var player in players)
                _format.AddPlayer(player);
        }
        else
        {
            DeactivateOnIdle();
        }

        await base.OnActivateAsync(cancellationToken);
    }
}
