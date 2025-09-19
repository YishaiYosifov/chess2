using Chess2.Api.GameLogic.Models;

namespace Chess2.Api.ArchivedGames.Entities;

public class MoveArchive
{
    public int Id { get; set; }

    public required int MoveNumber { get; set; }
    public required string San { get; set; }
    public required double TimeLeft { get; set; }

    public required byte FromIdx { get; set; }
    public required byte ToIdx { get; set; }
    public required ICollection<byte> Captures { get; set; } = [];
    public required ICollection<byte> Triggers { get; set; } = [];
    public required ICollection<byte> Intermediates { get; set; } = [];
    public required ICollection<MoveSideEffectArchive> SideEffects { get; set; } = [];
    public required PieceType? PromotesTo { get; set; }
}
