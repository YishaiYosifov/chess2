using Chess2.Api.Shared.Services;
using Chess2.Api.Profile.Entities;
using Microsoft.AspNetCore.Identity;

namespace Chess2.Api.Profile.Services;

public interface IUsernameGenerator
{
    Task<string> GenerateUniqueUsernameAsync();
}

public class UsernameGenerator(
    IUsernameWordsProvider usernameWordsProvider,
    UserManager<AuthedUser> userManager,
    IIRandomProvider randomProvider
) : IUsernameGenerator
{
    private readonly IUsernameWordsProvider _usernameWordsProvider = usernameWordsProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    private readonly IIRandomProvider _random = randomProvider;

    public async Task<string> GenerateUniqueUsernameAsync()
    {
        string username;
        while (true)
        {
            username = GenerateUsername();
            var isTaken = await IsUsernameTakenAsync(username);
            if (!isTaken)
                break;
        }

        return username;
    }

    private async Task<bool> IsUsernameTakenAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user is not null;
    }

    private string GenerateUsername()
    {
        var adjective = _usernameWordsProvider.Adjectives.ElementAt(
            _random.Next(_usernameWordsProvider.Adjectives.Count())
        );
        var noun = _usernameWordsProvider.Nouns.ElementAt(
            _random.Next(_usernameWordsProvider.Nouns.Count())
        );
        var suffix = GenerateNumberSuffix();
        return $"{adjective}-{noun}-{suffix}";
    }

    private int GenerateNumberSuffix()
    {
        int min = 1000;
        int max = 9999;
        return _random.Next(min, max + 1);
    }
}
