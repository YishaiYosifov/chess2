using System.Runtime.CompilerServices;
using Chess2.Api.Game.Services;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.GameSnapshot.Services;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Profile.Entities;
using Chess2.Api.Profile.Models;
using Chess2.Api.Shared.Services;
using Chess2.Api.Tournaments.Entities;
using Chess2.Api.Tournaments.Models;
using Chess2.Api.Tournaments.Repositories;
using Chess2.Api.UserRating.Services;

namespace Chess2.Api.Tournaments.Services;

public interface ITournamentPlayerService
{
    Task<TournamentPlayerState> AddPlayerAsync(
        AuthedUser user,
        Tournament tournament,
        CancellationToken token = default
    );
    IAsyncEnumerable<TournamentPlayerState> GetTournamentPlayersAsync(
        Tournament tournament,
        CancellationToken token = default
    );
    Task MatchPlayersAsync(
        UserId userId1,
        UserId userId2,
        Tournament tournament,
        CancellationToken token = default
    );
}

public class TournamentPlayerService(
    ITournamentPlayerRepository tournamentPlayerRepository,
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator,
    IUnitOfWork unitOfWork,
    IGameStarter gameStarter,
    TimeProvider timeProvider
) : ITournamentPlayerService
{
    private readonly ITournamentPlayerRepository _tournamentPlayerRepository =
        tournamentPlayerRepository;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IGameStarter _gameStarter = gameStarter;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<TournamentPlayerState> AddPlayerAsync(
        AuthedUser user,
        Tournament tournament,
        CancellationToken token = default
    )
    {
        var timeControl = _timeControlTranslator.FromSeconds(tournament.BaseSeconds);
        var rating = await _ratingService.GetRatingAsync(user.Id, timeControl, token);
        TournamentPlayer player = new()
        {
            UserId = user.Id,
            User = user,
            TournamentToken = tournament.TournamentToken,
            Rating = rating,
        };
        await _tournamentPlayerRepository.AddPlayerAsync(player, token);
        await _unitOfWork.CompleteAsync(token);

        return CreateSeekerFromPlayer(player, timeControl);
    }

    public async IAsyncEnumerable<TournamentPlayerState> GetTournamentPlayersAsync(
        Tournament tournament,
        [EnumeratorCancellation] CancellationToken token = default
    )
    {
        var timeControl = _timeControlTranslator.FromSeconds(tournament.BaseSeconds);
        var players = await _tournamentPlayerRepository.GetAllPlayersOfTournamentAsync(
            tournament.TournamentToken,
            token
        );

        foreach (var player in players)
        {
            yield return CreateSeekerFromPlayer(player, timeControl);
        }
    }

    public async Task MatchPlayersAsync(
        UserId userId1,
        UserId userId2,
        Tournament tournament,
        CancellationToken token = default
    )
    {
        PoolKey pool = new(
            PoolType.Rated,
            TimeControl: new(tournament.BaseSeconds, tournament.IncrementSeconds)
        );
        var gameToken = await _gameStarter.StartGameAsync(
            userId1,
            userId2,
            pool,
            fromTournament: tournament.TournamentToken,
            token
        );
        await _tournamentPlayerRepository.SetPlayersGameAsync(
            userId2,
            userId2,
            tournament.TournamentToken,
            gameToken,
            token
        );
    }

    private TournamentPlayerState CreateSeekerFromPlayer(
        TournamentPlayer player,
        TimeControl timeControl
    )
    {
        SeekerRating seekerRating = new(
            Value: player.Rating,
            AllowedRatingRange: null,
            timeControl
        );
        RatedSeeker seeker = new(
            UserId: player.UserId,
            UserName: player.User.UserName ?? "Unknown",
            ExcludeUserIds: player.LastOpponent is null ? [] : [player.LastOpponent.Value],
            CreatedAt: _timeProvider.GetUtcNow(),
            Rating: seekerRating
        );

        return new(seeker, player.Score, player.InGame);
    }
}
