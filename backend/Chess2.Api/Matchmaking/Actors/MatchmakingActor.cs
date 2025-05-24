using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Event;
using Chess2.Api.Matchmaking.Models;
using Chess2.Api.Shared.Models;

namespace Chess2.Api.Matchmaking.Actors;

public class MatchmakingActor : ReceiveActor, IWithTimers
{
    private readonly SortedDictionary<string, SeekInfo> _seekers = [];

    private readonly ILoggingAdapter _logger = Context.GetLogger();
    private readonly AppSettings _settings;

    // akka sets this internally
    public ITimerScheduler Timers { get; set; } = null!;

    public MatchmakingActor(AppSettings settings)
    {
        _settings = settings;

        Receive<MatchmakingCommands.CreateSeek>(HandleCreateSeek);
        Receive<MatchmakingCommands.CancelSeek>(HandleCancelSeek);
        Receive<MatchmakingCommands.MatchWave>(_ => HandleMatchWave());

        Receive<ReceiveTimeout>(_ => HandleTimeout());
    }

    public static Props PropsFor(AppSettings settings) =>
        Props.Create(() => new MatchmakingActor(settings));

    private void HandleCreateSeek(MatchmakingCommands.CreateSeek createSeek)
    {
        _logger.Info(
            "Received seek from {0} with rating {1}",
            createSeek.UserId,
            createSeek.Rating
        );

        var seek = new SeekInfo(createSeek.UserId, createSeek.Rating, Sender);
        _seekers.Add(seek.UserId, seek);
    }

    private void HandleCancelSeek(MatchmakingCommands.CancelSeek cancelSeek)
    {
        _logger.Info("Received cancel seek from {0}", cancelSeek.UserId);
        if (!_seekers.TryGetValue(cancelSeek.UserId, out var seek))
        {
            _logger.Warning("No seek found for user {0}", cancelSeek.UserId);
            return;
        }

        _seekers.Remove(seek.UserId);
    }

    private void HandleMatchWave()
    {
        var matches = CalculateMatches();
        foreach (var (seeker1, seeker2) in matches)
        {
            _logger.Info("Sending match to {0} and {1}", seeker1.UserId, seeker2.UserId);
            seeker1.Subscriber.Tell(new MatchmakingEvents.MatchFound(seeker2.UserId));
            seeker2.Subscriber.Tell(new MatchmakingEvents.MatchFound(seeker1.UserId));
        }
    }

    private List<(SeekInfo, SeekInfo)> CalculateMatches()
    {
        var matches = new List<(SeekInfo, SeekInfo)>();
        var alreadyMatched = new HashSet<string>();
        foreach (var seeker in _seekers.Values)
        {
            if (alreadyMatched.Contains(seeker.UserId))
                continue;

            SeekInfo? match = null;
            foreach (var potentialMatch in _seekers.Values)
            {
                if (seeker == potentialMatch || alreadyMatched.Contains(potentialMatch.UserId))
                    continue;

                var ratingDifference = Math.Abs(seeker.Rating - potentialMatch.Rating);
                var seeker1Range = CalculateRatingRange(seeker);
                var seeker2Range = CalculateRatingRange(potentialMatch);
                if (ratingDifference > seeker1Range || ratingDifference > seeker2Range)
                    continue;

                _logger.Info("Found match for {0} with {1}", seeker.UserId, potentialMatch.UserId);
                match = potentialMatch;
                break;
            }

            if (match is not null)
            {
                matches.Add((seeker, match));
                alreadyMatched.Add(seeker.UserId);
                alreadyMatched.Add(match.UserId);
            }
            else
            {
                seeker.WavesMissed++;
            }
        }

        foreach (var matchedSeeks in alreadyMatched)
            _seekers.Remove(matchedSeeks);

        return matches;
    }

    private int CalculateRatingRange(SeekInfo seeker) =>
        _settings.Game.StartingMatchRatingDifference
        + seeker.WavesMissed * _settings.Game.MatchRatingDifferenceGrowthPerWave;

    private void HandleTimeout()
    {
        if (_seekers.Count != 0)
            return;

        Context.Parent.Tell(new Passivate(PoisonPill.Instance));
    }

    protected override void PreStart()
    {
        Timers.StartPeriodicTimer(
            "wave",
            new MatchmakingCommands.MatchWave(),
            _settings.Game.MatchWaveEvery
        );
        Context.SetReceiveTimeout(TimeSpan.FromSeconds(30));
    }
}
