using Chess2.Api.Game.Grains;
using Chess2.Api.Game.Models;
using Chess2.Api.Shared.Services;

namespace Chess2.Api.Game.Services;

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
            var tokenTaken = await _grains.GetGrain<IGameGrain>(token).DoesGameExistAsync();
            if (!tokenTaken)
                return token;
        }
    }
}
