using AnarchyChess.Api.Game.Entities;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Game.Repositories;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.Profile.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.TestInfrastructure;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AnarchyChess.Api.Integration.Tests.LiveGameTests;

public class ChatMessageLoggerTests : BaseIntegrationTest
{
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly ChatMessageLogger _chatMessageLogger;

    public ChatMessageLoggerTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        var chatMessageRepository =
            Scope.ServiceProvider.GetRequiredService<IChatMessageRepository>();
        var unitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        _chatMessageLogger = new(chatMessageRepository, _timeProviderMock, unitOfWork);
    }

    [Fact]
    public async Task LogMessageAsync_adds_the_message_correctly()
    {
        GameToken gameToken = "game token";
        UserId userId = "user1";

        var message1Date = DateTime.UtcNow;
        _timeProviderMock.GetUtcNow().Returns(message1Date);
        await _chatMessageLogger.LogMessageAsync(gameToken, userId, "message1", CT);

        var message2Date = message1Date.AddDays(123);
        _timeProviderMock.GetUtcNow().Returns(message2Date);
        await _chatMessageLogger.LogMessageAsync(gameToken, userId, "message2", CT);

        DbContext.ChangeTracker.Clear();
        var messages = await DbContext.MessagesLogs.ToListAsync(CT);

        ChatMessage expectedMessage1 = new()
        {
            GameToken = gameToken,
            UserId = userId,
            Message = "message1",
            SentAt = message1Date,
        };
        ChatMessage expectedMessage2 = new()
        {
            GameToken = gameToken,
            UserId = userId,
            Message = "message2",
            SentAt = message2Date,
        };

        messages
            .Should()
            .BeEquivalentTo(
                [expectedMessage1, expectedMessage2],
                options => options.Excluding(m => m.Id)
            );
    }
}
