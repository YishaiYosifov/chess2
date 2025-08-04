using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using Chess2.Api.Shared.Models;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Chess2.Api.LiveGame.Services;

public interface IDrawRequestHandler
{
    void DecrementCooldown();
    DrawState GetState();
    bool HasPendingRequest(GameColor requester);
    ErrorOr<Success> RequestDraw(GameColor requester);
    bool TryDeclineDraw(GameColor by);
}

public class DrawRequestHandler(IOptions<AppSettings> settings) : IDrawRequestHandler
{
    private readonly int _drawCooldown = settings.Value.Game.DrawCooldown;

    private readonly Dictionary<GameColor, int> _activeCooldowns = [];
    private GameColor? _activeRequester;

    public ErrorOr<Success> RequestDraw(GameColor requester)
    {
        if (_activeRequester is not null)
            return GameErrors.DrawAlreadyRequested;

        if (_activeCooldowns.GetValueOrDefault(requester) > 0)
            return GameErrors.DrawOnCooldown;

        _activeRequester = requester;
        return Result.Success;
    }

    public bool TryDeclineDraw(GameColor by)
    {
        if (_activeRequester is null || _activeRequester == by)
            return false;

        var activeRequester = _activeRequester.Value;
        _activeCooldowns[activeRequester] = _drawCooldown;
        _activeRequester = null;
        return true;
    }

    public void DecrementCooldown()
    {
        foreach (var color in _activeCooldowns.Keys.ToList())
        {
            _activeCooldowns[color]--;

            if (_activeCooldowns[color] <= 0)
                _activeCooldowns.Remove(color);
        }
    }

    public bool HasPendingRequest(GameColor player) =>
        _activeRequester is not null && _activeRequester != player;

    public DrawState GetState() =>
        new(
            ActiveRequester: _activeRequester,
            WhiteCooldown: _activeCooldowns.GetValueOrDefault(GameColor.White),
            BlackCooldown: _activeCooldowns.GetValueOrDefault(GameColor.Black)
        );
}
