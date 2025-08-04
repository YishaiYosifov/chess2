using Chess2.Api.GameLogic.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.LiveGame.Services;
using Chess2.Api.TestInfrastructure.Utils;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Chess2.Api.Unit.Tests.LiveGameTests;

public class DrawRequestHandlerTests
{
    private readonly DrawRequestHandler _handler;
    private readonly int _drawRequestCooldownMoves;

    public DrawRequestHandlerTests()
    {
        var settings = AppSettingsLoader.LoadAppSettings();
        _drawRequestCooldownMoves = settings.Game.DrawCooldown;

        _handler = new(Options.Create(settings));
    }

    [Fact]
    public void RequestDraw_succeeds_when_no_active_request_and_not_on_cooldown()
    {
        var result = _handler.RequestDraw(GameColor.White);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public void RequestDraw_fails_when_request_already_active()
    {
        _handler.RequestDraw(GameColor.White);

        var result = _handler.RequestDraw(GameColor.Black);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.DrawAlreadyRequested);
    }

    [Fact]
    public void RequestDraw_fails_when_requester_is_on_cooldown()
    {
        _handler.RequestDraw(GameColor.White);
        _handler.TryDeclineDraw(GameColor.Black);

        var result = _handler.RequestDraw(GameColor.White);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.DrawOnCooldown);
    }

    [Fact]
    public void TryDeclineDraw_returns_false_when_no_active_request()
    {
        var result = _handler.TryDeclineDraw(GameColor.White);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryDeclineDraw_returns_false_when_decliner_is_the_requester()
    {
        _handler.RequestDraw(GameColor.White);

        var result = _handler.TryDeclineDraw(GameColor.White);

        result.Should().BeFalse();

        var state = _handler.GetState();
        state.ActiveRequester.Should().Be(GameColor.White);
    }

    [Fact]
    public void TryDeclineDraw_clears_request_and_sets_cooldown_on_requester()
    {
        _handler.RequestDraw(GameColor.Black);

        var success = _handler.TryDeclineDraw(GameColor.White);

        success.Should().BeTrue();

        var state = _handler.GetState();
        state.ActiveRequester.Should().BeNull();
        state.BlackCooldown.Should().Be(_drawRequestCooldownMoves);
        state.WhiteCooldown.Should().Be(0);
    }

    [Fact]
    public void HasPendingRequest_returns_true_for_opponent()
    {
        _handler.RequestDraw(GameColor.White);

        _handler.HasPendingRequest(GameColor.Black).Should().BeTrue();
        _handler.HasPendingRequest(GameColor.White).Should().BeFalse();
    }

    [Fact]
    public void GetDrawState_returns_correct_active_requester_and_cooldown()
    {
        _handler.RequestDraw(GameColor.Black);

        var state = _handler.GetState();

        state.ActiveRequester.Should().Be(GameColor.Black);
        state.WhiteCooldown.Should().Be(0);
        state.BlackCooldown.Should().Be(0);
    }

    [Fact]
    public void DecrementCooldown_decreases_each_cooldown_and_removes_expired_entries()
    {
        _handler.RequestDraw(GameColor.White);
        _handler.TryDeclineDraw(GameColor.Black);

        for (var i = 0; i < _drawRequestCooldownMoves - 1; i++)
        {
            _handler.DecrementCooldown();
            _handler.RequestDraw(GameColor.White).IsError.Should().BeTrue();
        }

        _handler.DecrementCooldown();
        _handler.GetState().WhiteCooldown.Should().Be(0);
        _handler.GetState().BlackCooldown.Should().Be(0);
        _handler.RequestDraw(GameColor.White).IsError.Should().BeFalse();
    }
}
