namespace Chess2.Api.Game.Entities;

public class MoveArchive
{
    public int Id { get; set; }

    public required int MoveNumber { get; set; }
    public required string EncodedMove { get; set; }
    public required string San { get; set; }
    public required double TimeLeft { get; set; }
}
