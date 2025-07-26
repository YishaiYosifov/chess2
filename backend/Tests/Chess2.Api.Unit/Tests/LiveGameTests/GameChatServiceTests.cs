using Akka.Actor;
using Akka.Hosting;
using Akka.TestKit;
using Chess2.Api.Auth.Services;
using Chess2.Api.LiveGame.Actors;
using Chess2.Api.LiveGame.Models;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Fakes;
using Chess2.Api.TestInfrastructure.Utils;
using ErrorOr;
using FluentAssertions;
using NSubstitute;
using System.Security.Claims;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class GameChatServiceTests : BaseActorTest
{
    private readonly IAuthService _authServiceMock = Substitute.For<IAuthService>();
    private readonly TestProbe _gameChatActorProbe;

    private readonly GameChatService _gameChatService;

    public GameChatServiceTests()
    {
        _gameChatActorProbe = CreateTestProbe();

        var requiredActorMock = Substitute.For<IRequiredActor<GameChatActor>>();
        requiredActorMock.ActorRef.Returns(_gameChatActorProbe);

        _gameChatService = new(_authServiceMock, requiredActorMock);
    }

    [Fact]
    public async Task JoinChat_rejects_guest_users()
    {
        var guestClaims = new ClaimsPrincipal(new ClaimsIdentity());
        _authServiceMock.GetLoggedInUserAsync(guestClaims).Returns(Error.Unauthorized());

        var result = await _gameChatService.JoinChat("some-game", "conn-1", guestClaims, CT);

        result.IsError.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(Error.Unauthorized());
        await _gameChatActorProbe.ExpectNoMsgAsync(CT);
    }

    [Fact]
    public async Task JoinChat_allows_authed_users()
    {
        var user = new AuthedUserFaker().Generate();
        var userClaims = ClaimUtils.CreateUserClaims(user.Id);
        _authServiceMock.GetLoggedInUserAsync(userClaims).Returns(user);

        var joinTask = _gameChatService.JoinChat("some-game", "conn-1", userClaims, CT);

        var joinChatMsg = await _gameChatActorProbe.ExpectMsgAsync<GameChatCommands.JoinChat>(
            cancellationToken: CT
        );
        joinChatMsg
            .Should()
            .BeEquivalentTo(
                new GameChatCommands.JoinChat("some-game", "conn-1", user.Id, user.UserName!)
            );
        _gameChatActorProbe.Sender.Tell(new GameChatEvents.UserJoined());

        var result = await joinTask;

        result.IsError.Should().BeFalse();
    }
}
