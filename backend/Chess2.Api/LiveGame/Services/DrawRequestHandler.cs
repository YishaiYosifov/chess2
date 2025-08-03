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
    DrawState GetDrawState();
    bool HasPendingRequest(GameColor requester);
    ErrorOr<Success> RequestDraw(GameColor requester);
    bool TryDeclineDraw();
}

public class DrawRequestHandler(IOptions<AppSettings> settings) : IDrawRequestHandler
{
    private readonly int _drawRequestCooldownMoves = settings.Value.Game.DrawRequestCooldownMoves;

    private readonly Dictionary<GameColor, int> _drawCooldown = [];
    private GameColor? _activeRequester;

    public ErrorOr<Success> RequestDraw(GameColor requester)
    {
        if (_activeRequester is not null)
            return GameErrors.DrawAlreadyRequested;

        if (_drawCooldown.GetValueOrDefault(requester) > 0)
            return GameErrors.DrawOnCooldown;

        _activeRequester = requester;
        return Result.Success;
    }

    public bool TryDeclineDraw()
    {
        if (_activeRequester is null)
            return false;

        var activeRequester = _activeRequester.Value;
        _drawCooldown[activeRequester] = _drawRequestCooldownMoves;
        _activeRequester = null;
        return true;
    }

    public void DecrementCooldown()
    {
        foreach (var color in _drawCooldown.Keys.ToList())
        {
            _drawCooldown[color]--;

            if (_drawCooldown[color] <= 0)
                _drawCooldown.Remove(color);
        }
    }

    public bool HasPendingRequest(GameColor player) =>
        _activeRequester is not null && _activeRequester != player;

    public DrawState GetDrawState() =>
        new(ActiveRequester: _activeRequester, DrawCooldown: _drawCooldown.AsReadOnly());
}
