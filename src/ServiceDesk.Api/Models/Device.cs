using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDesk.Api.Models;

[Table("devices")]
public class Device
{
    [Column("id")] public int Id { get; set; }
    [Column("name")] public string Name { get; set; } ="";
    [Column("type")] public string Type { get; set; } ="";
    [Column("hostname")] public string? Hostname { get; set; }
    [Column("ip_address")] public string? IpAddress { get; set; }
    [Column("location")] public string? Location { get; set; }
    [Column("owner_id")] public int? OwnerId { get; set; }
    [Column("warranty_until")] public DateOnly? WarrantyUntil { get; set; }
    [Column("status")] public string Status { get; set; } ="aktiv";
}