using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ServiceDesk.Api.Models;

[Table("users")]
public class User
{
    [Column("id")] public int Id { get; set; }
    [Column("username")] public string Username { get; set; } ="";
    [Column("email")] public string Email { get; set; } ="";
    [JsonIgnore] [Column("password_hash")] public string PasswordHash { get; set; } ="";
    [Column("role")] public string Role { get; set; } ="nutzer";
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}