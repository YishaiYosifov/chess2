namespace Chess2.Api.ArchivedGames.Entities;

public class MoveSideEffectArchive
{
    public int Id { get; set; }

    public required byte FromIdx { get; set; }
    public required byte ToIdx { get; set; }
}
