using AnarchyChess.Api.Infrastructure;

namespace AnarchyChess.Api.Shared.Services;

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
