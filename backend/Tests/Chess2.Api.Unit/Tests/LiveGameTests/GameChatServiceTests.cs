using Akka.Actor;
using Akka.Hosting;
using Akka.TestKit;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using FluentAssertions;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameChatServiceTests : BaseActorTest
{
    private readonly TestProbe _gameChatActorProbe;
    private readonly GameChatService _gameChatService;

    private const string GameToken = "test game";
    private const string UserId = "user123";
    private const string ConnectionId = "conn 123";

    public GameChatServiceTests()
    {
        _gameChatActorProbe = CreateTestProbe();

        var requiredActorMock = Substitute.For<IRequiredActor<GameChatActor>>();
        requiredActorMock.ActorRef.Returns(_gameChatActorProbe);

        _gameChatService = new GameChatService(requiredActorMock);
    }

    [Fact]
    public async Task JoinChat_sends_JoinChat_command_to_actor()
    {
        var joinTask = _gameChatService.JoinChat(
            gameToken: GameToken,
            userId: UserId,
            connectionId: ConnectionId,
            CT
        );

        var msg = await _gameChatActorProbe.ExpectMsgAsync<GameChatCommands.JoinChat>(
            cancellationToken: CT
        );
        msg.Should()
            .BeEquivalentTo(
                new GameChatCommands.JoinChat(
                    GameToken: GameToken,
                    ConnectionId: ConnectionId,
                    UserId: UserId
                )
            );

        _gameChatActorProbe.Sender.Tell(new GameChatReplies.UserJoined());
        var result = await joinTask;

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task LeaveChat_sends_LeaveChat_command_to_actor()
    {
        var resultTask = _gameChatService.LeaveChat(
            gameToken: GameToken,
            userId: UserId,
            connectionId: ConnectionId,
            CT
        );

        var msg = await _gameChatActorProbe.ExpectMsgAsync<GameChatCommands.LeaveChat>(
            cancellationToken: CT
        );
        msg.Should()
            .BeEquivalentTo(
                new GameChatCommands.LeaveChat(
                    GameToken: GameToken,
                    ConnectionId: ConnectionId,
                    UserId: UserId
                )
            );

        _gameChatActorProbe.Sender.Tell(new GameChatReplies.UserLeft());

        var result = await resultTask;
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task SendMessage_sends_SendMessage_command_to_actor()
    {
        var message = "test message";

        var task = _gameChatService.SendMessage(
            gameToken: GameToken,
            userId: UserId,
            connectionId: ConnectionId,
            message: message,
            CT
        );

        var msg = await _gameChatActorProbe.ExpectMsgAsync<GameChatCommands.SendMessage>(
            cancellationToken: CT
        );
        msg.Should()
            .BeEquivalentTo(
                new GameChatCommands.SendMessage(
                    GameToken: GameToken,
                    ConnectionId: ConnectionId,
                    UserId: UserId,
                    message
                )
            );

        _gameChatActorProbe.Sender.Tell(new GameChatReplies.MessageSent());

        var result = await task;
        result.IsError.Should().BeFalse();
    }
}
