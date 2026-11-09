using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDesk.Api.Models;

[Table("tickets")]
public class Ticket
{
    [Column("id")] public int Id { get; set; }
    [Column("title")] public string Title { get; set; } ="";
    [Column("description")] public string Description { get; set; } ="";
    [Column("status")] public string Status { get; set; } ="offen";
    [Column("priority")] public string Priority { get; set; } ="mittel";
    [Column("created_by")] public int? CreatedBy { get; set; }
    [Column("assigned_to")] public int? AssignedTo { get; set; }
    [Column("device_id")] public int? DeviceId { get; set; }
    [Column("source")] public string Source { get; set; } ="manuell";
    [Column("solution_note")] public string? SolutionNote { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("resolved_at")] public DateTime? ResolvedAt { get; set; }
}

