using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceDesk.Api.Models;

[Table("measurements")]
public class Measurement
{
    [Column("id")] public int Id { get; set; }
    [Column("device_id")] public int DeviceId { get; set; }
    [Column("cpu_percent")] public decimal? CpuPercent { get; set; }
    [Column("ram_percent")] public decimal? RamPercent { get; set; }
    [Column("disk_percent")] public decimal? DiskPercent { get; set; }
    [Column("is_reachable")] public bool IsReachable { get; set; }
    [Column("measured_at")] public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
}