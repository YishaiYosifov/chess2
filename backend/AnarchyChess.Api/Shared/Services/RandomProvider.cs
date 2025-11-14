namespace AnarchyChess.Api.Shared.Services;

public interface IRandomProvider
{
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
    void NextBytes(byte[] buffer);
    T NextItem<T>(IEnumerable<T> enumerable);
}

public class RandomProvider : IRandomProvider
{
    private readonly Random _random = new();

    public int Next() => _random.Next();

    public int Next(int maxValue) => _random.Next(maxValue);

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public double NextDouble() => _random.NextDouble();

    public void NextBytes(byte[] buffer) => _random.NextBytes(buffer);

    public T NextItem<T>(IEnumerable<T> items) => items.ElementAt(_random.Next(items.Count()));
}
