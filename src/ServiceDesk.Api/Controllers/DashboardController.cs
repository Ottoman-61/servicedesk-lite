using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;

namespace ServiceDesk.Api.Controllers;

public record DeviceStatus(int Deviceid, string Name, string Type, string Status, decimal? CpuPercen, decimal? RamPercent, decimal? DiskPercent, bool? IsReachable, DateTime? MeasuredAt);

public record DashboardDto(int OpenTickets, int InProgressTickets, int CriticalOpenTickets, int AgentTickets, int ResolvedLast7Days, List<DeviceStatus> Devices);

[ApiController]
[Authorize]
[Route("dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ServiceDeskContext _db;

    public DashboardController(ServiceDeskContext db) => _db = db;

    
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var tickets = _db.Tickets.AsNoTracking();

        var open = await tickets.CountAsync(t => t.Status == "offen");
        var inProgress = await tickets.CountAsync(t => t.Status == "in_bearbeitung");
        var critical = await tickets.CountAsync(t => t.Priority == "kritisch" && (t.Status == "offen" || t.Status == "in_bearbeitung"));
        var fromAgent = await tickets.CountAsync(t => t.Source == "agent" && (t.Status == "offen" || t.Status == "in_bearbeitung"));

        var since = DateTime.UtcNow.AddDays(-7);
        var resolved = await tickets.CountAsync(t => t.ResolvedAt != null && t.ResolvedAt >= since);

        var devices = await _db.Devices.AsNoTracking().OrderBy(d => d.Name).ToListAsync();
        var states = new List<DeviceStatus>();
        foreach (var d in devices)
        {
            var m = await _db.Measurements.AsNoTracking()
                .Where(x => x.DeviceId == d.Id)
                .OrderByDescending(x => x.MeasuredAt)
                .FirstOrDefaultAsync();

                states.Add(new DeviceStatus(d.Id, d.Name, d.Type, d.Status, m?.CpuPercent, m?.RamPercent, m?.DiskPercent, m?.IsReachable, m?.MeasuredAt));
        }

        return new DashboardDto(open, inProgress, critical, fromAgent, resolved, states);
    }
}