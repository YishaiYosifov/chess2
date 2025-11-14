using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Game.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Game.Models.GameEndedEvent")]
public record GameEndedEvent(GameToken GameToken, GameResultData EndStatus);
