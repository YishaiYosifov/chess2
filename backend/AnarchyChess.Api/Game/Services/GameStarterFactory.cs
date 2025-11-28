namespace AnarchyChess.Api.Game.Services;

public interface IGameStarterFactory
{
    ValueTask<T> UseAsync<T>(
        Func<IGameStarter, CancellationToken, Task<T>> action,
        CancellationToken token = default
    );
}

public class GameStarterFactory(IServiceScopeFactory scopeFactory) : IGameStarterFactory
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async ValueTask<T> UseAsync<T>(
        Func<IGameStarter, CancellationToken, Task<T>> action,
        CancellationToken token = default
    )
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var starter = scope.ServiceProvider.GetRequiredService<IGameStarter>();
        return await action(starter, token);
    }
}
