using Chess2.Api.LiveGame.Entities;
using Chess2.Api.LiveGame.Repositories;
using Chess2.Api.Shared.Services;

namespace Chess2.Api.LiveGame.Services;

public interface IChatMessageLogger
{
    Task LogMessageAsync(
        string gameToken,
        string userId,
        string message,
        CancellationToken token = default
    );
}

public class ChatMessageLogger(
    IChatMessageRepository chatMessageRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork
) : IChatMessageLogger
{
    private readonly IChatMessageRepository _chatMessageRepository = chatMessageRepository;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task LogMessageAsync(
        string gameToken,
        string userId,
        string message,
        CancellationToken token = default
    )
    {
        ChatMessage chatMessage = new()
        {
            GameToken = gameToken,
            UserId = userId,
            Message = message,
            SentAt = _timeProvider.GetUtcNow().UtcDateTime,
        };
        await _chatMessageRepository.AddMessageAsync(chatMessage, token);
        await _unitOfWork.CompleteAsync(token);
    }
}
