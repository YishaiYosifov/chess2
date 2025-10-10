using Chess2.Api.LiveGame.Grains;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.Shared.Services;

namespace Chess2.Api.LiveGame.Services;

public interface IGameTokenGenerator
{
    Task<GameToken> GenerateUniqueGameToken();
}

public class GameTokenGenerator(IGrainFactory grains, IRandomCodeGenerator randomCodeGenerator)
    : IGameTokenGenerator
{
    private readonly IGrainFactory _grains = grains;
    private readonly IRandomCodeGenerator _randomCodeGenerator = randomCodeGenerator;

    public async Task<GameToken> GenerateUniqueGameToken()
    {
        while (true)
        {
            var token = _randomCodeGenerator.GenerateBase62Code(16);
            var isGameOngoing = await _grains.GetGrain<IGameGrain>(token).IsGameOngoingAsync();
            if (!isGameOngoing)
                return token;
        }
    }
}
