using Chess2.Api.LiveGame.Entities;
using Chess2.Api.LiveGame.Repositories;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Services;
using Chess2.Api.TestInfrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Chess2.Api.Integration.Tests.LiveGameTests;

public class ChatMessageLoggerTests : BaseIntegrationTest
{
    private readonly TimeProvider _timeProviderMock = Substitute.For<TimeProvider>();

    private readonly ChatMessageLogger _chatMessageLogger;

    public ChatMessageLoggerTests(Chess2WebApplicationFactory factory)
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
        const string gameToken = "game token";
        const string userId = "user1";

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
