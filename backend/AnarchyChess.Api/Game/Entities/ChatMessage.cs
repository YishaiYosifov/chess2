using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.Profile.Models;

namespace AnarchyChess.Api.Game.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public required UserId UserId { get; set; }
    public required GameToken GameToken { get; set; }

    public required string Message { get; set; }
    public required DateTime SentAt { get; set; }
}
