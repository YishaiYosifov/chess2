using Microsoft.EntityFrameworkCore;

namespace AnarchyChess.Api.Donations.Entities;

[PrimaryKey(nameof(Email))]
public class Donation
{
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required decimal TotalAmount { get; set; }
}
