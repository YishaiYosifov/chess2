namespace Chess2.Api.Infrastructure.Extensions;

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
}
