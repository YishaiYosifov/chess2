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
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Entities;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameChatActorTests : BaseActorTest
{
    private readonly UserManager<AuthedUser> _userManagerMock =
        UserManagerMockUtils.CreateUserManagerMock();
    private readonly ILiveGameService _liveGameServiceMock = Substitute.For<ILiveGameService>();
    private readonly IChatMessageLogger _chatMessageLoggerMock =
        Substitute.For<IChatMessageLogger>();
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

    private readonly AuthedUser _whiteUser;
    private readonly AuthedUser _blackUser;

    private readonly IActorRef _gameChatActor;
    private readonly TestProbe _probe;

    private const string WhiteUserId = "white-user";
    private const string BlackUserId = "black-user";
    private const string TestGameToken = "test-game";

    private const string ConnectionId = "test-connection-id";

    public GameChatActorTests()
    {
        _whiteUser = new AuthedUserFaker()
            .RuleFor(x => x.Id, _whitePlayer.UserId)
            .RuleFor(x => x.UserName, _whitePlayer.UserName)
            .Generate();
        _blackUser = new AuthedUserFaker()
            .RuleFor(x => x.Id, _blackPlayer.UserId)
            .RuleFor(x => x.UserName, _blackPlayer.UserName)
            .Generate();
        _userManagerMock.FindByIdAsync(_whiteUser.Id).Returns(_whiteUser);
        _userManagerMock.FindByIdAsync(_blackUser.Id).Returns(_blackUser);

        var serviceScopeMock = Substitute.For<IServiceScope>();
        serviceScopeMock.ServiceProvider.Returns(_serviceProviderMock);

        var serviceScopeFactoryMock = Fixture.Create<IServiceScopeFactory>();
        serviceScopeFactoryMock.CreateScope().Returns(serviceScopeMock);

        _serviceProviderMock
            .GetService(typeof(IServiceScopeFactory))
            .Returns(serviceScopeFactoryMock);
        _serviceProviderMock.GetService(typeof(ILiveGameService)).Returns(_liveGameServiceMock);
        _serviceProviderMock.GetService(typeof(IChatMessageLogger)).Returns(_chatMessageLoggerMock);
        _serviceProviderMock.GetService(typeof(UserManager<AuthedUser>)).Returns(_userManagerMock);

        var settingOptions = Fixture.Create<IOptions<AppSettings>>();
        _settings = settingOptions.Value.Game.Chat;

        _liveGameServiceMock
            .GetGamePlayersAsync(TestGameToken)
            .Returns(new GameResponses.GamePlayers(_whitePlayer, _blackPlayer));

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
        _gameChatActor.Tell(
            new GameChatCommands.JoinChat(
                GameToken: TestGameToken,
                ConnectionId: ConnectionId,
                UserId: userId
            ),
            _probe.Ref
        );
        await _probe.ExpectMsgAsync<GameChatEvents.UserJoined>(cancellationToken: CT);

        await _gameChatNotifierMock
            .Received(1)
            .JoinChatAsync(TestGameToken, ConnectionId, isPlaying);
    }

    [Theory]
    [InlineData(WhiteUserId, true)]
    [InlineData(BlackUserId, true)]
    [InlineData("another-user", false)]
    public async Task LeaveChat_when_user_in_chat_removes_user(string userId, bool isPlaying)
    {
        _gameChatActor.Tell(
            new GameChatCommands.LeaveChat(
                GameToken: TestGameToken,
                ConnectionId: ConnectionId,
                UserId: userId
            ),
            _probe.Ref
        );

        await _probe.ExpectMsgAsync<GameChatEvents.UserLeft>(cancellationToken: CT);
        await _gameChatNotifierMock
            .Received(1)
            .LeaveChatAsync(TestGameToken, ConnectionId, isPlaying);
    }

    [Theory]
    [InlineData(WhiteUserId, true)]
    [InlineData(BlackUserId, true)]
    [InlineData("another-user", false)]
    public async Task SendMessage_when_user_in_chat_sends_message(string userId, bool isPlaying)
    {
        const string message = "test message";

        var user = new AuthedUserFaker().RuleFor(x => x.Id, userId).Generate();
        _userManagerMock.FindByIdAsync(userId).Returns(user);

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
                ConnectionId: ConnectionId,
                UserId: userId,
                Message: message
            ),
            _probe.Ref
        );

        await _probe.ExpectMsgAsync<GameChatEvents.MessageSent>(cancellationToken: CT);

        await _gameChatNotifierMock
            .Received(1)
            .SendMessageAsync(
                TestGameToken,
                user.UserName!,
                ConnectionId,
                newCooldown,
                message,
                isPlaying
            );
        await AwaitAssertAsync(
            () =>
                _chatMessageLoggerMock
                    .Received(1)
                    .LogMessageAsync(TestGameToken, userId, message, Arg.Any<CancellationToken>()),
            cancellationToken: CT
        );
    }

    [Fact]
    public async Task SendMessage_caches_username()
    {
        const string message = "test message 123";

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(
                GameToken: TestGameToken,
                ConnectionId: ConnectionId,
                UserId: _whitePlayer.UserId,
                Message: message
            ),
            _probe.Ref
        );
        await _probe.ExpectMsgAsync<GameChatEvents.MessageSent>(cancellationToken: CT);

        _userManagerMock.FindByIdAsync(_whitePlayer.UserId).Returns((AuthedUser?)null);

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(
                GameToken: TestGameToken,
                ConnectionId: ConnectionId,
                UserId: _whitePlayer.UserId,
                Message: message
            ),
            _probe.Ref
        );
        await _probe.ExpectMsgAsync<GameChatEvents.MessageSent>(cancellationToken: CT);

        await _gameChatNotifierMock
            .Received(2)
            .SendMessageAsync(
                TestGameToken,
                _whiteUser.UserName!,
                ConnectionId,
                TimeSpan.Zero,
                message,
                isPlaying: true
            );
    }

    [Fact]
    public async Task SendMessage_returns_error_when_user_is_not_found()
    {
        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(
                GameToken: TestGameToken,
                ConnectionId: ConnectionId,
                UserId: "random id",
                Message: "message"
            ),
            _probe.Ref
        );
        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(cancellationToken: CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameChatErrors.InvalidUser);
    }

    [Fact]
    public async Task SendMessage_with_cooldown_returns_error()
    {
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
                ConnectionId: ConnectionId,
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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendMessage_rejects_empty_messages(string msg)
    {
        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(
                GameToken: TestGameToken,
                ConnectionId: ConnectionId,
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
        string longMsg = new('a', _settings.MaxMessageLength + 1);

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(TestGameToken, ConnectionId, WhiteUserId, longMsg),
            _probe.Ref
        );

        var result = await _probe.ExpectMsgAsync<ErrorOr<object>>(cancellationToken: CT);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameChatErrors.InvalidMessage);
    }

    [Fact]
    public async Task SendMessage_treats_chatters_as_spectators_when_players_are_not_found()
    {
        _liveGameServiceMock
            .GetGamePlayersAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(GameErrors.GameNotFound);

        _gameChatActor.Tell(
            new GameChatCommands.SendMessage(TestGameToken, ConnectionId, WhiteUserId, "test"),
            _probe
        );
        await _probe.ExpectMsgAsync<GameChatEvents.MessageSent>(cancellationToken: CT);

        await _gameChatNotifierMock
            .Received(1)
            .SendMessageAsync(
                TestGameToken,
                _whitePlayer.UserName,
                ConnectionId,
                TimeSpan.Zero,
                "test",
                isPlaying: false
            );
    }
}
