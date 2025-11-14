namespace AnarchyChess.Api.GameLogic.Models;

/// <summary>
/// Defines the priority of the forced moves
/// Higher values indicate higher priority
/// </summary>
public enum ForcedMovePriority
{
    None = 0,
    UnderagePawn = 1,
    EnPassant = 2,
}
