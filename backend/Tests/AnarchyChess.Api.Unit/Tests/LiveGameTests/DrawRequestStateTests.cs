using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.Game.Errors;
using AnarchyChess.Api.Game.Services;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class DrawRequestStateTests
{
    private readonly DrawRequestState _state = new();
    private const int DrawRequestCooldownMoves = 20;

    [Fact]
    public void RequestDraw_succeeds_when_no_active_request_and_not_on_cooldown()
    {
        var result = _state.RequestDraw(GameColor.White);

        result.IsError.Should().BeFalse();
    }

    [Fact]
    public void RequestDraw_fails_when_request_already_active()
    {
        _state.RequestDraw(GameColor.White);

        var result = _state.RequestDraw(GameColor.Black);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.DrawAlreadyRequested);
    }

    [Fact]
    public void RequestDraw_fails_when_requester_is_on_cooldown()
    {
        _state.RequestDraw(GameColor.White);
        _state.TryDeclineDraw(GameColor.Black, DrawRequestCooldownMoves);

        var result = _state.RequestDraw(GameColor.White);

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(GameErrors.DrawOnCooldown);
    }

    [Fact]
    public void TryDeclineDraw_returns_false_when_no_active_request()
    {
        var result = _state.TryDeclineDraw(GameColor.White, DrawRequestCooldownMoves);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryDeclineDraw_returns_false_when_decliner_is_the_requester()
    {
        _state.RequestDraw(GameColor.White);

        var result = _state.TryDeclineDraw(GameColor.White, DrawRequestCooldownMoves);

        result.Should().BeFalse();

        var state = _state.GetState();
        state.ActiveRequester.Should().Be(GameColor.White);
    }

    [Fact]
    public void TryDeclineDraw_clears_request_and_sets_cooldown_on_requester()
    {
        _state.RequestDraw(GameColor.Black);

        var success = _state.TryDeclineDraw(GameColor.White, DrawRequestCooldownMoves);

        success.Should().BeTrue();

        var state = _state.GetState();
        state.ActiveRequester.Should().BeNull();
        state.BlackCooldown.Should().Be(DrawRequestCooldownMoves);
        state.WhiteCooldown.Should().Be(0);
    }

    [Fact]
    public void HasPendingRequest_returns_true_for_opponent()
    {
        _state.RequestDraw(GameColor.White);

        _state.HasPendingRequest(GameColor.Black).Should().BeTrue();
        _state.HasPendingRequest(GameColor.White).Should().BeFalse();
    }

    [Fact]
    public void GetDrawState_returns_correct_active_requester_and_cooldown()
    {
        _state.RequestDraw(GameColor.Black);

        var state = _state.GetState();

        state.ActiveRequester.Should().Be(GameColor.Black);
        state.WhiteCooldown.Should().Be(0);
        state.BlackCooldown.Should().Be(0);
    }

    [Fact]
    public void DecrementCooldown_decreases_each_cooldown_and_removes_expired_entries()
    {
        _state.RequestDraw(GameColor.White);
        _state.TryDeclineDraw(GameColor.Black, DrawRequestCooldownMoves);

        for (var i = 0; i < DrawRequestCooldownMoves - 1; i++)
        {
            _state.DecrementCooldown();
            _state.RequestDraw(GameColor.White).IsError.Should().BeTrue();
        }

        _state.DecrementCooldown();
        _state.GetState().WhiteCooldown.Should().Be(0);
        _state.GetState().BlackCooldown.Should().Be(0);
        _state.RequestDraw(GameColor.White).IsError.Should().BeFalse();
    }
}
