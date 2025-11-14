using AnarchyChess.Api.Game.Entities;
using AnarchyChess.Api.Game.Repositories;
using AnarchyChess.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class ChatMessageRepositoryTests : BaseIntegrationTest
{
    private readonly IChatMessageRepository _chatMessageRepository;

    public ChatMessageRepositoryTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _chatMessageRepository = Scope.ServiceProvider.GetRequiredService<IChatMessageRepository>();
    }

    [Fact]
    public async Task AddMessageAsync_adds_the_message_correctly()
    {
        ChatMessage message = new()
        {
            UserId = "user 1",
            GameToken = "game token",
            Message = "test message",
            SentAt = DateTime.UtcNow,
        };

        await _chatMessageRepository.AddMessageAsync(message, CT);

        await DbContext.SaveChangesAsync(CT);
        var messages = await DbContext.MessagesLogs.ToListAsync(CT);
        messages.Should().ContainSingle().Which.Should().BeEquivalentTo(message);
    }
}
