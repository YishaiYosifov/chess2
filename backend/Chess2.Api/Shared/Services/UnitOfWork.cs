using Chess2.Api.Infrastructure;

namespace Chess2.Api.Shared.Services;

public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken token = default);
}

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task CompleteAsync(CancellationToken token = default) =>
        _dbContext.SaveChangesAsync(token);
}
