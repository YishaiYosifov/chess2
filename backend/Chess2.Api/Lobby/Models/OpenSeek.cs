using Chess2.Api.GameSnapshot.Models;
using Chess2.Api.Matchmaking.Models;

namespace Chess2.Api.Lobby.Models;

public record OpenSeek(SeekKey SeekKey, string UserName, TimeControl TimeControl, int? Rating);
