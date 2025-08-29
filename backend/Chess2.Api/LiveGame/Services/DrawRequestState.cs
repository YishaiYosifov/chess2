﻿using Chess2.Api.GameLogic.Models;
using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.LiveGame.Errors;
using ErrorOr;

namespace Chess2.Api.LiveGame.Services;

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Services.DrawRequestState")]
public class DrawRequestState
{
    [Id(0)]
    private readonly Dictionary<GameColor, int> _activeCooldowns = [];

    [Id(1)]
    public GameColor? ActiveRequester { get; private set; }

    public ErrorOr<Success> RequestDraw(GameColor requester)
    {
        if (ActiveRequester is not null)
            return GameErrors.DrawAlreadyRequested;

        if (_activeCooldowns.GetValueOrDefault(requester) > 0)
            return GameErrors.DrawOnCooldown;

        ActiveRequester = requester;
        return Result.Success;
    }

    public bool TryDeclineDraw(GameColor by, int drawCooldown)
    {
        if (ActiveRequester is null || ActiveRequester == by)
            return false;

        var activeRequester = ActiveRequester.Value;
        _activeCooldowns[activeRequester] = drawCooldown;
        ActiveRequester = null;
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
        ActiveRequester is not null && ActiveRequester != player;

    public DrawState GetState() =>
        new(
            ActiveRequester: ActiveRequester,
            WhiteCooldown: _activeCooldowns.GetValueOrDefault(GameColor.White),
            BlackCooldown: _activeCooldowns.GetValueOrDefault(GameColor.Black)
        );
}
