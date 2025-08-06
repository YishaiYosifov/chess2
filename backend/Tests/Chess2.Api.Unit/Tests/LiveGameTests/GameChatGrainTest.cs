using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Grains;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.Shared.Models;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using Chess2.Api.Users.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameChatGrainTest : BaseGrainTest
{
    private readonly ChatSettings _settings;

    private readonly UserManager<AuthedUser> _userManagerMock =
        UserManagerMockUtils.CreateUserManagerMock();
    private readonly ILiveGameService _liveGameServiceMock = Substitute.For<ILiveGameService>();
    private readonly IChatMessageLogger _chatMessageLoggerMock =
        Substitute.For<IChatMessageLogger>();
    private readonly IGameChatNotifier _gameChatNotifierMock = Substitute.For<IGameChatNotifier>();
    private readonly IChatRateLimiter _chatRateLimiterMock = Substitute.For<IChatRateLimiter>();

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

    private const string WhiteUserId = "white-user";
    private const string BlackUserId = "black-user";
    private const string TestGameToken = "test-game";

    private const string ConnectionId = "test-connection-id";

    public GameChatGrainTest()
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

        var settingOptions = AppSettingsLoader.LoadAppSettings();
        _settings = settingOptions.Game.Chat;

        _liveGameServiceMock
            .GetGamePlayersAsync(TestGameToken, CT)
            .Returns(new GameReplies.GamePlayers(_whitePlayer, _blackPlayer));

        _chatRateLimiterMock
            .ShouldAllowRequest(Arg.Any<string>(), out Arg.Any<TimeSpan>())
            .Returns(x =>
            {
                x[1] = TimeSpan.Zero;
                return true;
            });

        Silo.ServiceProvider.AddService(_userManagerMock);
        Silo.ServiceProvider.AddService(Options.Create(settingOptions));
        Silo.ServiceProvider.AddService(_liveGameServiceMock);
        Silo.ServiceProvider.AddService(_chatRateLimiterMock);
        Silo.ServiceProvider.AddService(_gameChatNotifierMock);
        Silo.ServiceProvider.AddService(_chatMessageLoggerMock);
    }

    [Theory]
    [InlineData(WhiteUserId, true)]
    [InlineData(BlackUserId, true)]
    [InlineData("another-user", false)]
    public async Task JoinChat_provides_notifier_correct_information(string userId, bool isPlaying)
    {
        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        await grain.JoinChatAsync(ConnectionId, userId, CT);

        await _gameChatNotifierMock
            .Received(1)
            .JoinChatAsync(TestGameToken, ConnectionId, isPlaying, CT);
    }

    [Theory]
    [InlineData(WhiteUserId, true)]
    [InlineData(BlackUserId, true)]
    [InlineData("another-user", false)]
    public async Task LeaveChat_when_user_in_chat_removes_user(string userId, bool isPlaying)
    {
        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        await grain.LeaveChatAsync(ConnectionId, userId, CT);

        await _gameChatNotifierMock
            .Received(1)
            .LeaveChatAsync(TestGameToken, ConnectionId, isPlaying, CT);
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

        var cooldown = TimeSpan.FromSeconds(5);
        _chatRateLimiterMock
            .ShouldAllowRequest(userId, out Arg.Any<TimeSpan>())
            .Returns(x =>
            {
                x[1] = cooldown;
                return true;
            });

        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        var result = await grain.SendMessageAsync(ConnectionId, userId, message, CT);

        result.IsError.Should().BeFalse();

        await _gameChatNotifierMock
            .Received(1)
            .SendMessageAsync(
                TestGameToken,
                user.UserName!,
                ConnectionId,
                cooldown,
                message,
                isPlaying
            );

        await _chatMessageLoggerMock
            .Received(1)
            .LogMessageAsync(TestGameToken, userId, message, CT);
    }

    [Fact]
    public async Task SendMessage_caches_username()
    {
        const string message = "test message 123";

        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        var result1 = await grain.SendMessageAsync(ConnectionId, WhiteUserId, message, CT);
        result1.IsError.Should().BeFalse();

        _userManagerMock.FindByIdAsync(WhiteUserId).Returns((AuthedUser?)null);

        var result2 = await grain.SendMessageAsync(ConnectionId, WhiteUserId, message, CT);
        result2.IsError.Should().BeFalse();

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
        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        var result = await grain.SendMessageAsync(ConnectionId, "random id", "message", CT);

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameChatErrors.InvalidUser);
    }

    [Fact]
    public async Task SendMessage_with_cooldown_returns_error()
    {
        var cooldown = TimeSpan.FromSeconds(5);
        _chatRateLimiterMock
            .ShouldAllowRequest(WhiteUserId, out Arg.Any<TimeSpan>())
            .Returns(x =>
            {
                x[1] = cooldown;
                return false;
            });

        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        var result = await grain.SendMessageAsync(ConnectionId, WhiteUserId, "test", CT);

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameChatErrors.OnCooldown);

        await _gameChatNotifierMock
            .DidNotReceive()
            .SendMessageAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<string>(),
                Arg.Any<bool>()
            );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendMessage_rejects_empty_messages(string msg)
    {
        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        var result = await grain.SendMessageAsync(ConnectionId, WhiteUserId, msg, CT);

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

        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        var result = await grain.SendMessageAsync(ConnectionId, WhiteUserId, longMsg, CT);

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().Be(GameChatErrors.InvalidMessage);
    }

    [Fact]
    public async Task SendMessage_treats_chatters_as_spectators_when_players_are_not_found()
    {
        _liveGameServiceMock
            .GetGamePlayersAsync(Arg.Any<string>(), CT)
            .Returns(GameErrors.GameNotFound);

        var grain = await Silo.CreateGrainAsync<GameChatGrain>(TestGameToken);
        var result = await grain.SendMessageAsync(ConnectionId, WhiteUserId, "test", CT);

        result.IsError.Should().BeFalse();

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
