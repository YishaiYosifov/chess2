namespace Chess2.Api.TestInfrastructure.Utils;

public static class CancellationTokenExtensions
{
    public static CancellationToken WithTimeout(this CancellationToken token, TimeSpan timeout)
    {
        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            token,
            timeoutCts.Token
        );

        return linkedCts.Token;
    }
}
