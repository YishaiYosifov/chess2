using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.Game.Models;

[GenerateSerializer]
[Alias("Chess2.Api.Game.Models.GameEndedEvent")]
public record GameEndedEvent(GameToken GameToken, GameResultData EndStatus);
