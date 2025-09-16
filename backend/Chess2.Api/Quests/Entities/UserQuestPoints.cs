using System.ComponentModel.DataAnnotations.Schema;
using Chess2.Api.Profile.Entities;

namespace Chess2.Api.Quests.Entities;

public class UserQuestPoints
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required AuthedUser User { get; set; }

    public int Points { get; set; }
}
