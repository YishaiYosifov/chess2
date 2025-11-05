using System.Runtime.CompilerServices;
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
    Task IncrementScoreForAsync(
        UserId userId,
        TournamentToken tournamentToken,
        int incrementBy,
        CancellationToken token = default
    );
    Task RemovePlayerAsync(
        UserId userId,
        TournamentToken tournamentToken,
        CancellationToken token = default
    );
}

public class TournamentPlayerService(
    ITournamentPlayerRepository tournamentPlayerRepository,
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider
) : ITournamentPlayerService
{
    private readonly ITournamentPlayerRepository _tournamentPlayerRepository =
        tournamentPlayerRepository;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
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

    public async Task IncrementScoreForAsync(
        UserId userId,
        TournamentToken tournamentToken,
        int incrementBy,
        CancellationToken token = default
    )
    {
        await _tournamentPlayerRepository.IncrementScoreForAsync(
            userId,
            tournamentToken,
            incrementBy,
            token
        );
        await _unitOfWork.CompleteAsync(token);
    }

    public async Task RemovePlayerAsync(
        UserId userId,
        TournamentToken tournamentToken,
        CancellationToken token = default
    )
    {
        await _tournamentPlayerRepository.RemovePlayerFromTournamentAsync(
            userId,
            tournamentToken,
            token
        );
        await _unitOfWork.CompleteAsync(token);
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

        return new(seeker, player.Score);
    }
}
