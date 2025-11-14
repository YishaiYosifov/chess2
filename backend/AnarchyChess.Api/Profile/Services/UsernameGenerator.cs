using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Shared.Services;
using Microsoft.AspNetCore.Identity;

namespace AnarchyChess.Api.Profile.Services;

public interface IUsernameGenerator
{
    Task<string> GenerateUniqueUsernameAsync();
}

public class UsernameGenerator(
    IUsernameWordsProvider usernameWordsProvider,
    UserManager<AuthedUser> userManager,
    IRandomProvider randomProvider
) : IUsernameGenerator
{
    private readonly IUsernameWordsProvider _usernameWordsProvider = usernameWordsProvider;
    private readonly UserManager<AuthedUser> _userManager = userManager;

    private readonly IRandomProvider _random = randomProvider;

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
        var adjective = _random.NextItem(_usernameWordsProvider.Adjectives);
        var noun = _random.NextItem(_usernameWordsProvider.Nouns);

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
