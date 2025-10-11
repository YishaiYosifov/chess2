namespace Chess2.Api.TestInfrastructure.Utils;

public static class CancellationTokenExtensions
{
    public static CancellationTokenSource WithTimeout(
        this CancellationToken token,
        TimeSpan timeout
    )
    {
        var timeoutCts = new CancellationTokenSource(timeout);
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutCts.Token);

        return linkedCts;
    }
}
