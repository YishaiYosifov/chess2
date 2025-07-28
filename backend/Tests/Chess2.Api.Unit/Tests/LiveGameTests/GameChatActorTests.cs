using Akka.Actor;
using Akka.TestKit;
using AutoFixture;
using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameChatActorTests : BaseActorTest
{
    private readonly ILiveGameService _liveGameServiceMock = Substitute.For<ILiveGameService>();
    private readonly IServiceProvider _serviceProviderMock = Substitute.For<IServiceProvider>();
    private readonly IGameChatNotifier _gameChatNotifierMock = Substitute.For<IGameChatNotifier>();
    private readonly IChatRateLimiter _chatRateLimiterMock = Substitute.For<IChatRateLimiter>();
    private readonly ChatSettings _settings;

    private readonly GamePlayer _whitePlayer = new GamePlayerFaker(GameColor.White).RuleFor(
        x => x.UserId,
        WhiteUserId
    );
    private readonly GamePlayer _blackPlayer = new GamePlayerFaker(GameColor.Black).RuleFor(
        x => x.UserId,
        BlackUserId
    );

    private readonly IActorRef _gameChatActor;
    private readonly TestProbe _probe;

    private const string WhiteUserId = "white-user";
    private const string BlackUserId = "black-user";
    private const string TestGameToken = "test-game";

    public GameChatActorTests()
    {
        var serviceScopeMock = Substitute.For<IServiceScope>();
        serviceScopeMock.ServiceProvider.Returns(_serviceProviderMock);

        var serviceScopeFactoryMock = Fixture.Create<IServiceScopeFactory>();
        serviceScopeFactoryMock.CreateScope().Returns(serviceScopeMock);

        _serviceProviderMock
            .GetService(typeof(IServiceScopeFactory))
            .Returns(serviceScopeFactoryMock);
        _serviceProviderMock.GetService(typeof(ILiveGameService)).Returns(_liveGameServiceMock);

        var settingOptions = Fixture.Create<IOptions<AppSettings>>();
        _settings = settingOptions.Value.Game.Chat;

        _liveGameServiceMock
            .GetGamePlayersAsync(TestGameToken)
            .Returns(new GameEvents.GamePlayersEvent(_whitePlayer, _blackPlayer));

        _chatRateLimiterMock
            .ShouldAllowRequest(Arg.Any<string>(), out Arg.Any<TimeSpan>())
            .Returns(x =>
            {
                x[1] = TimeSpan.Zero;
                return true;
            });

        _probe = CreateTestProbe();
        _gameChatActor = Sys.ActorOf(
            Props.Create(
                () =>
                    new GameChatActor(
                        TestGameToken,
                        _serviceProviderMock,
                        settingOptions,
                        _gameChatNotifierMock,
                        _chatRateLimiterMock
                    )
            )
        );
    }

    [Theory]
    [InlineData(WhiteUserId, true)]
    [InlineData(BlackUserId, true)]
    [InlineData("another-user", false)]
    public async Task JoinChat_provides_notifier_correct_information(string userId, bool isPlaying)
    {
        const string connId = "conn";
        const string username = "user";
        await JoinChat(userId, connId: connId, username: username);

        await _gameChatNotifierMock.Received(1).JoinChatAsync(TestGameToken, connId, isPlaying);
    }

    [Fact]
    public async Task JoinChat_when_user_already_joined_returns_error()
    {
        await JoinChat(WhiteUserId, connId: "conn1", username: "user1");

        _gameChatActor.Tell(
            new GameChatCommands.JoinChat(
                GameToken: TestGameToken,
                UserId: WhiteUserId,
                ConnectionId: "conn2",
                UserName: "user2"
            ),
            _probe.Ref
        );
        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(cancellationToken: CT);

        result.IsError.Should().BeTrue();
        result
            .Errors.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(GameChatErrors.UserAlreadyJoined);
    }

    [Theory]
    [InlineData(WhiteUserId, true)]
    [InlineData(BlackUserId, true)]
    [InlineData("another-user", false)]
    public async Task LeaveChat_when_user_in_chat_removes_user(string userId, bool isPlaying)
    {
        const string connId = "conn";

        await JoinChat(userId, connId: connId);

        _gameChatActor.Tell(
            new GameChatCommands.LeaveChat(GameToken: TestGameToken, UserId: userId),
            _probe.Ref
        );

        await _probe.ExpectMsgAsync<GameChatEvents.UserLeft>(cancellationToken: CT);
        await _gameChatNotifierMock.Received(1).LeaveChatAsync(TestGameToken, connId, isPlaying);
    }

    [Fact]
    public async Task LeaveChat_when_user_not_in_chat_returns_error()
    {
        _gameChatActor.Tell(
            new GameChatCommands.LeaveChat(GameToken: TestGameToken, UserId: WhiteUserId),
            _probe.Ref
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(cancellationToken: CT);

        result.IsError.Should().BeTrue();
        result
            .Errors.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(GameChatErrors.UserNotInChat);
    }

    [Theory]
    [InlineData(WhiteUserId, true)]
    [InlineData(BlackUserId, true)]
    [InlineData("another-user", false)]
    public async Task SendMessage_when_user_in_chat_sends_message(string userId, bool isPlaying)
    {
        const string username = "username";
        const string message = "hello world";
        const string connId = "conn";

        await JoinChat(userId, connId: connId, username: username);
        await JoinChat("some-random-id", username: "some random guy");

        var newCooldown = TimeSpan.FromSeconds(5);
        _chatRateLimiterMock
            .ShouldAllowRequest(userId, out Arg.Any<TimeSpan>())
            .Returns(x =>
            {
                x[1] = newCooldown;
                return true;
            });

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(
                GameToken: TestGameToken,
                UserId: userId,
                Message: message
            ),
            _probe.Ref
        );

        await _probe.ExpectMsgAsync<GameChatEvents.MessageSent>(cancellationToken: CT);

        await _gameChatNotifierMock
            .Received(1)
            .SendMessageAsync(TestGameToken, username, connId, newCooldown, message, isPlaying);
    }

    [Fact]
    public async Task SendMessage_with_cooldown_returns_error()
    {
        await JoinChat(_whitePlayer.UserId);

        var newCooldown = TimeSpan.FromSeconds(5);
        _chatRateLimiterMock
            .ShouldAllowRequest(_whitePlayer.UserId, out Arg.Any<TimeSpan>())
            .Returns(x =>
            {
                x[1] = newCooldown;
                return false;
            });

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(
                GameToken: TestGameToken,
                UserId: _whitePlayer.UserId,
                Message: "test"
            ),
            _probe.Ref
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(cancellationToken: CT);

        result.IsError.Should().BeTrue();
        result.Errors.Single().Should().Be(GameChatErrors.OnCooldown);

        _gameChatNotifierMock.Received(0);
    }

    [Fact]
    public async Task SendMessage_when_user_not_in_chat_returns_error()
    {
        await JoinChat("random guy");

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(
                GameToken: TestGameToken,
                UserId: WhiteUserId,
                Message: "hello"
            ),
            _probe.Ref
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(cancellationToken: CT);

        result.IsError.Should().BeTrue();
        result
            .Errors.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(GameChatErrors.UserNotInChat);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendMessage_rejects_empty_messages(string msg)
    {
        await JoinChat(WhiteUserId);

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(
                GameToken: TestGameToken,
                UserId: WhiteUserId,
                Message: msg
            ),
            _probe.Ref
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(cancellationToken: CT);

        result.IsError.Should().BeTrue();
        result
            .Errors.Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(GameChatErrors.InvalidMessage);
    }

    [Fact]
    public async Task SendMessage_rejects_message_exceeding_max_length()
    {
        var longMsg = new string('a', _settings.MaxMessageLength + 1);
        await JoinChat(WhiteUserId);

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(TestGameToken, WhiteUserId, longMsg),
            _probe.Ref
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(cancellationToken: CT);

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameChatErrors.InvalidMessage);
    }

    private async Task JoinChat(string userId, string connId = "conn", string username = "username")
    {
        _gameChatActor.Tell(
            new GameChatCommands.JoinChat(
                GameToken: TestGameToken,
                ConnectionId: connId,
                UserId: userId,
                UserName: username
            ),
            _probe.Ref
        );
        await _probe.ExpectMsgAsync<GameChatEvents.UserJoined>(cancellationToken: CT);
    }
}
