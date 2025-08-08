using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.DrawState")]
public record DrawState(
    GameColor? ActiveRequester = null,
    int WhiteCooldown = 0,
    int BlackCooldown = 0
);
