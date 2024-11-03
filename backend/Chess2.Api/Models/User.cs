using System.ComponentModel.DataAnnotations.Schema;

namespace Chess2.Api.Models;

public class User
{
    public int UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
}
