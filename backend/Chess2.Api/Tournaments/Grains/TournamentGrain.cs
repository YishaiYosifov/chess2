using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Infrastructure;
using Chess2.Api.Profile.Models;
using Chess2.Api.Tournaments.Errors;
using ErrorOr;

namespace Chess2.Api.Tournaments.Grains;

[Alias("Chess2.Api.Tournaments.Grains.ITournamentGrain")]
public interface ITournamentGrain : IGrainWithStringKey
{
    [Alias("CreateTournamentAsync")]
    Task<ErrorOr<Created>> CreateTournamentAsync(
        UserId hostedBy,
        TimeControlSettings timeControl,
        CancellationToken token = default
    );

    [Alias("JoinAsync")]
    Task<ErrorOr<Success>> JoinAsync(UserId userId, CancellationToken token = default);
}

[GenerateSerializer]
[Alias("Chess2.Api.Tournaments.Grains.TournamentState")]
public class Tournament
{
    [Id(0)]
    public required UserId HostedBy { get; init; }

    [Id(1)]
    public required TimeControlSettings TimeControl { get; init; }
}

[GenerateSerializer]
[Alias("Chess2.Api.Tournaments.Grains.TournamentGrainState")]
public class TournamentGrainState
{
    [Id(0)]
    public Tournament? Tournament { get; set; }
}

public class TournamentGrain(
    ILogger<TournamentGrain> logger,
    [PersistentState(TournamentGrain.StateName, StorageNames.TournamentState)]
        IPersistentState<TournamentGrainState> state
) : Grain, IGrainBase, ITournamentGrain
{
    public const string StateName = "tournament";

    private readonly ILogger<TournamentGrain> _logger = logger;
    private readonly IPersistentState<TournamentGrainState> _state = state;

    public async Task<ErrorOr<Created>> CreateTournamentAsync(
        UserId hostedBy,
        TimeControlSettings timeControl,
        CancellationToken token = default
    )
    {
        if (_state.State.Tournament is not null)
            return TournamentErrors.TournamentAlreadyExists;

        _state.State.Tournament = new Tournament { HostedBy = hostedBy, TimeControl = timeControl };
        await _state.WriteStateAsync(token);

        return Result.Created;
    }

    public Task<ErrorOr<Success>> JoinAsync(UserId userId, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
