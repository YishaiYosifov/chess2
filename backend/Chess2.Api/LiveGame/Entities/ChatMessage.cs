using Chess2.Api.LiveGame.Models;
using Chess2.Api.Profile.Models;

namespace Chess2.Api.LiveGame.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }
    public required GameToken GameToken { get; set; }

    public required string Message { get; set; }
    public required DateTime SentAt { get; set; }
}
