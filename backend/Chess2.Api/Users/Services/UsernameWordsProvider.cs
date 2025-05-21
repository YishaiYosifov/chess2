namespace Chess2.Api.Users.Services;

public interface IUsernameWordsProvider
{
    IEnumerable<string> Adjectives { get; }
    IEnumerable<string> Nouns { get; }
}

public class UsernameWordsProvider : IUsernameWordsProvider
{
    public IEnumerable<string> Adjectives { get; }
    public IEnumerable<string> Nouns { get; }

    public UsernameWordsProvider()
    {
        var adjectivesPath = Path.Combine(AppContext.BaseDirectory, "Data", "adjectives.txt");
        Adjectives = LoadWords(adjectivesPath);

        var nounsPath = Path.Combine(AppContext.BaseDirectory, "Data", "nouns.txt");
        Nouns = LoadWords(nounsPath);
    }

    private static string[] LoadWords(string path)
    {
        var words = File.ReadAllLines(path);
        if (words.Length == 0)
            throw new InvalidOperationException($"No words found in {path}");
        return words;
    }
}
