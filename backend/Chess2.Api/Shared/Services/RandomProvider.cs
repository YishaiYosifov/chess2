namespace Chess2.Api.Shared.Services;

public interface IIRandomProvider
{
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
    void NextBytes(byte[] buffer);
}

public class RandomProvider : IIRandomProvider
{
    private readonly Random _random = new();

    public int Next() => _random.Next();

    public int Next(int maxValue) => _random.Next(maxValue);

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public double NextDouble() => _random.NextDouble();

    public void NextBytes(byte[] buffer) => _random.NextBytes(buffer);
}
