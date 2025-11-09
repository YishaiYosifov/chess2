using Chess2.Api.Game.Grains;
using Chess2.Api.Game.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Errors;
using Chess2.Api.Tournaments.Extensions;
using Chess2.Api.Tournaments.Models;
using Chess2.Api.Tournaments.Repositories;
using Chess2.Api.Tournaments.Services;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Orleans.Streams;

namespace Chess2.Api.Tournaments.Grains;

[Alias("Chess2.Api.Tournaments.Grains.ITournamentGrain`1")]
public interface ITournamentGrain : IGrainWithStringKey
{
    [Alias("CreateAsync")]
    Task<ErrorOr<Created>> CreateAsync(
        UserId hostedBy,
        TimeControlSettings timeControl,
        TournamentFormat format,
        CancellationToken token = default
    );

    [Alias("JoinAsync")]
    Task<ErrorOr<Created>> JoinAsync(UserId userId, CancellationToken token = default);

    [Alias("LeaveAsync")]
    Task<ErrorOr<Deleted>> LeaveAsync(UserId userId, CancellationToken token = default);

    [Alias("StartAsync")]
    Task<ErrorOr<Success>> StartAsync(UserId startedBy, CancellationToken token = default);

    [Alias("GetPlayerAsync")]
    Task<Dictionary<UserId, TournamentPlayerState>> GetPlayerAsync();
}

[GenerateSerializer]
[Alias("Chess2.Api.Tournaments.Grains.TournamentGrainState")]
public class TournamentGrainState
{
    [Id(0)]
    public StreamSubscriptionHandle<GameEndedEvent>? GameEndedEventSubscription { get; set; }
}

public class TournamentGrain(
    ILogger<TournamentGrain> logger,
    [PersistentState(TournamentGrain.StateName, Storage.StorageProvider)]
        IPersistentState<TournamentGrainState> state,
    UserManager<AuthedUser> userManager,
    ITournamentPlayerService tournamentPlayerService,
    ITournamentRepository tournamentRepository,
    ITournamentPlayerRepository tournamentPlayerRepository,
    IUnitOfWork unitOfWork
) : Grain, IGrainBase, ITournamentGrain
{
    public const string StateName = "tournament";

    private readonly ILogger<TournamentGrain> _logger = logger;
    private readonly IPersistentState<TournamentGrainState> _state = state;

    private readonly ITournamentPlayerService _tournamentPlayerService = tournamentPlayerService;
    private readonly ITournamentPlayerRepository _tournamentPlayerRepository =
        tournamentPlayerRepository;
    private readonly ITournamentRepository _tournamentRepository = tournamentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    private readonly Dictionary<UserId, TournamentPlayerState> _players = [];
    private Tournament? _tournament;

    public async Task<ErrorOr<Created>> CreateAsync(
        UserId hostedBy,
        TimeControlSettings timeControl,
        TournamentFormat format,
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
            Format = format,
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
        await GrainFactory.GetTournamentFormatGrain(_tournament).StartAsync(token);

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
        _players.TryAdd(userId, player);
        await GrainFactory
            .GetTournamentFormatGrain(_tournament)
            .PlayerAvailableAsync(player, token);

        return Result.Created;
    }

    public async Task<ErrorOr<Deleted>> LeaveAsync(UserId userId, CancellationToken token = default)
    {
        if (_tournament is null)
            return TournamentErrors.TournamentNotFound;

        if (!_players.ContainsKey(userId))
            return TournamentErrors.NotPartOfTournament;

        _players.Remove(userId);
        await _tournamentPlayerRepository.RemovePlayerFromTournamentAsync(
            userId,
            _tournament.TournamentToken,
            token
        );
        await _unitOfWork.CompleteAsync(token);
        await GrainFactory
            .GetTournamentFormatGrain(_tournament)
            .PlayerUnavailableAsync(userId, token);

        return Result.Deleted;
    }

    public Task<Dictionary<UserId, TournamentPlayerState>> GetPlayerAsync() =>
        Task.FromResult(_players);

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider(Streaming.StreamProvider);
        var stream = streamProvider.GetStream<GameEndedEvent>(
            nameof(GameEndedEvent),
            this.GetPrimaryKeyString()
        );

        if (_state.State.GameEndedEventSubscription is null)
        {
            _state.State.GameEndedEventSubscription = await stream.SubscribeAsync(OnGameEndedAsync);
            await _state.WriteStateAsync(cancellationToken);
        }
        else
        {
            await _state.State.GameEndedEventSubscription.ResumeAsync(OnGameEndedAsync);
        }

        _tournament = await _tournamentRepository.GetByTokenAsync(
            tournamentToken: this.GetPrimaryKeyString(),
            cancellationToken
        );
        await InitializePlayers(cancellationToken);
        await base.OnActivateAsync(cancellationToken);
    }

    private async Task OnGameEndedAsync(GameEndedEvent @event, StreamSequenceToken? token = null)
    {
        if (_tournament is null)
            return;

        var gameGrain = GrainFactory.GetGrain<IGameGrain>(@event.GameToken);
        var playersResult = await gameGrain.GetPlayersAsync();
        if (playersResult.IsError)
        {
            _logger.LogWarning(
                "Could not find players for game {GameToken} on tournament {TournamentToken}, {Errors}",
                @event.GameToken,
                _tournament.TournamentToken,
                playersResult.Errors
            );
            return;
        }
        var gamePlayers = playersResult.Value;

        var formatGrain = GrainFactory.GetTournamentFormatGrain(_tournament);
        if (_players.TryGetValue(gamePlayers.WhitePlayer.UserId, out var whitePlayer))
            await formatGrain.PlayerAvailableAsync(whitePlayer);
        if (_players.TryGetValue(gamePlayers.BlackPlayer.UserId, out var blackPlayer))
            await formatGrain.PlayerAvailableAsync(blackPlayer);
    }

    private async Task InitializePlayers(CancellationToken token = default)
    {
        if (_tournament is null)
        {
            DeactivateOnIdle();
            return;
        }

        var players = _tournamentPlayerService.GetTournamentPlayersAsync(_tournament, token);
        await foreach (var player in players)
        {
            _players[player.Seeker.UserId] = player;
        }
    }
}
