using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.DrawState")]
public record DrawState(
    GameColor? ActiveRequester = null,
    int WhiteCooldown = 0,
    int BlackCooldown = 0
);
