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

public interface ITournamentService
{
    Task<TournamentPlayerState> AddPlayerAsync(
        AuthedUser user,
        TournamentToken tournamentToken,
        TimeControlSettings timeControlSettings,
        CancellationToken token = default
    );
    Task<Dictionary<UserId, TournamentPlayerState>> GetTournamentPlayers(
        TournamentToken tournamentToken,
        TimeControlSettings timeControlSettings,
        CancellationToken token = default
    );
    Task RegisterTournamentAsync(
        TournamentToken tournamentToken,
        UserId hostedBy,
        TimeControlSettings timeControl,
        CancellationToken token = default
    );
    Task RemovePlayerAsync(
        UserId userId,
        TournamentToken tournamentToken,
        CancellationToken token = default
    );
}

public class TournamentService(
    ITournamentRepository tournamentRepository,
    ITournamentPlayerRepository tournamentPlayerRepository,
    IRatingService ratingService,
    ITimeControlTranslator timeControlTranslator,
    IUnitOfWork unitOfWork
) : ITournamentService
{
    private readonly ITournamentRepository _tournamentRepository = tournamentRepository;
    private readonly ITournamentPlayerRepository _tournamentPlayerRepository =
        tournamentPlayerRepository;
    private readonly IRatingService _ratingService = ratingService;
    private readonly ITimeControlTranslator _timeControlTranslator = timeControlTranslator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task RegisterTournamentAsync(
        TournamentToken tournamentToken,
        UserId hostedBy,
        TimeControlSettings timeControl,
        CancellationToken token = default
    )
    {
        Tournament tournament = new()
        {
            TournamentToken = tournamentToken,
            HostedBy = hostedBy,
            BaseSeconds = timeControl.BaseSeconds,
            IncrementSeconds = timeControl.IncrementSeconds,
        };
        await _tournamentRepository.AddTournamentAsync(tournament, token);
        await _unitOfWork.CompleteAsync(token);
    }

    public async Task<TournamentPlayerState> AddPlayerAsync(
        AuthedUser user,
        TournamentToken tournamentToken,
        TimeControlSettings timeControlSettings,
        CancellationToken token = default
    )
    {
        var timeControl = _timeControlTranslator.FromSeconds(timeControlSettings.BaseSeconds);
        var rating = await _ratingService.GetRatingAsync(user.Id, timeControl, token);
        TournamentPlayer player = new()
        {
            UserId = user.Id,
            User = user,
            TournamentToken = tournamentToken,
            Rating = rating,
        };
        await _tournamentPlayerRepository.AddPlayerAsync(player, token);
        await _unitOfWork.CompleteAsync(token);

        return CreateSeekerFromPlayer(player, timeControl);
    }

    public async Task RemovePlayerAsync(
        UserId userId,
        TournamentToken tournamentToken,
        CancellationToken token = default
    )
    {
        await _tournamentPlayerRepository.RemovePlayerByIdAsync(userId, tournamentToken, token);
        await _unitOfWork.CompleteAsync(token);
    }

    public async Task<Dictionary<UserId, TournamentPlayerState>> GetTournamentPlayers(
        TournamentToken tournamentToken,
        TimeControlSettings timeControlSettings,
        CancellationToken token = default
    )
    {
        var timeControl = _timeControlTranslator.FromSeconds(timeControlSettings.BaseSeconds);
        var players = await _tournamentPlayerRepository.GetAllPlayersOfTournamentAsync(
            tournamentToken,
            token
        );

        Dictionary<UserId, TournamentPlayerState> result = [];
        foreach (var player in players)
        {
            result[player.UserId] = CreateSeekerFromPlayer(player, timeControl);
        }
        return result;
    }

    private static TournamentPlayerState CreateSeekerFromPlayer(
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
            ExcludeUserIds: player.LastOpponent is not null ? [player.LastOpponent.Value] : [],
            CreatedAt: DateTimeOffset.UtcNow,
            Rating: seekerRating
        );

        return new(seeker, player.Score);
    }
}
