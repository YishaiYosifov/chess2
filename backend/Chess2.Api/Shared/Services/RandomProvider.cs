namespace Chess2.Api.Shared.Services;

public interface IRandomProvider
{
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    double NextDouble();
    void NextBytes(byte[] buffer);
    T NextItem<T>(IEnumerable<T> enumerable);
    T NextWeighted<T>(IDictionary<int, T> items);
}

public class RandomProvider : IRandomProvider
{
    private readonly Random _random = new();

    public int Next() => _random.Next();

    public int Next(int maxValue) => _random.Next(maxValue);

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public double NextDouble() => _random.NextDouble();

    public void NextBytes(byte[] buffer) => _random.NextBytes(buffer);

    public T NextWeighted<T>(IDictionary<int, T> items)
    {
        var sortedItems = items.OrderBy(x => x.Key).ToList();

        int totalWeight = items.Sum(x => x.Key);
        int randomWeight = _random.Next(totalWeight);
        foreach ((int weight, T value) in sortedItems)
        {
            if (randomWeight < weight)
                return value;
            randomWeight -= weight;
        }

        return sortedItems[^1].Value;
    }

    public T NextItem<T>(IEnumerable<T> items) => items.ElementAt(_random.Next(items.Count()));
}
