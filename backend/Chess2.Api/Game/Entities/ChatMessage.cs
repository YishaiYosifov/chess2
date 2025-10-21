using Chess2.Api.Game.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.Game.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }
    public required GameToken GameToken { get; set; }

    public required string Message { get; set; }
    public required DateTime SentAt { get; set; }
}
