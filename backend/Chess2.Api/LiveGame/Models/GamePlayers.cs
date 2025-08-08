using Chess2.Api.GameSnapshot.Models;

namespace Chess2.Api.LiveGame.Models;

[GenerateSerializer]
[Alias("Chess2.Api.LiveGame.Models.GamePlayers")]
public record GamePlayers(GamePlayer WhitePlayer, GamePlayer BlackPlayer);
