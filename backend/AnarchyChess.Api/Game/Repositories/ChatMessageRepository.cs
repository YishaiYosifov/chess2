using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Game.Entities;

namespace AnarchyChess.Api.Game.Repositories;

public interface IChatMessageRepository
{
    Task AddMessageAsync(ChatMessage message, CancellationToken token = default);
}

public class ChatMessageRepository(ApplicationDbContext dbContext) : IChatMessageRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task AddMessageAsync(ChatMessage message, CancellationToken token = default)
    {
        await _dbContext.MessagesLogs.AddAsync(message, token);
    }
}
