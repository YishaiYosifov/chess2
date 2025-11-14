using System.Diagnostics;

namespace AnarchyChess.Api.Shared.Services;

public interface IStopwatchProvider
{
    void Start();
    void Stop();
    void Reset();
    void Restart();

    TimeSpan Elapsed { get; }
}

public class StopwatchProvider : IStopwatchProvider
{
    private readonly Stopwatch _stopwatch = new();

    public void Start() => _stopwatch.Start();

    public void Stop() => _stopwatch.Stop();

    public void Reset() => _stopwatch.Reset();

    public void Restart() => _stopwatch.Restart();

    public TimeSpan Elapsed => _stopwatch.Elapsed;
}
