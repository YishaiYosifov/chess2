namespace Chess2.Api.LiveGame.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string GameToken { get; set; }

    public required string Message { get; set; }
    public required DateTime SentAt { get; set; }
}
