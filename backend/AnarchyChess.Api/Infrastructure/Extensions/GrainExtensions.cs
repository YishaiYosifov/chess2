using Orleans.Streams;

namespace AnarchyChess.Api.Infrastructure.Extensions;

public static class GrainExtensions
{
    public static TGrainInterface AsSafeReference<TGrainInterface>(this IAddressable grain)
    {
        try
        {
            return grain.AsReference<TGrainInterface>();
        }
        catch (ArgumentException ex)
            when (ex.Message.Contains("Passing a half baked grain as an argument"))
        {
            return (TGrainInterface)grain;
        }
    }

    public static async Task SubscribeOrResumeAsync<T>(
        this IAsyncStream<T> stream,
        Func<T, StreamSequenceToken, Task> callback
    )
    {
        var existingHandles = await stream.GetAllSubscriptionHandles();
        if (existingHandles.Count == 0)
        {
            await stream.SubscribeAsync(callback);
            return;
        }

        foreach (var handle in existingHandles)
        {
            await handle.ResumeAsync(callback);
        }
    }
}
