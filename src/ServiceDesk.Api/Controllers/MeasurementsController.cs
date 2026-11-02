using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;
using ServiceDesk.Api.Models;

namespace ServiceDesk.Api.Controllers;

public record CreateMeasurementDto(int DeviceId, decimal? CpuPercent, decimal? RamPercent, decimal? DiskPercent, bool IsReachable);

[ApiController]
[Route("[controller]")]
public class MeasurementsController : ControllerBase
{
    private readonly ServiceDeskContext _db;
    private readonly IConfiguration _config;

    public MeasurementsController(ServiceDeskContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMeasurementDto dto)
    {
        var expected = _config["Agent:ApiKey"];
        if (string.IsNullOrEmpty(expected) || Request.Headers["X-Api-Key"].ToString() != expected)
            return Unauthorized();

        if (!await _db.Devices.AnyAsync(d => d.Id == dto.DeviceId))
            return BadRequest("Geraet existiert nicht.");

        _db.Measurements.Add(new Measurement
        {
            DeviceId = dto.DeviceId,
            CpuPercent = dto.CpuPercent,
            RamPercent = dto.RamPercent,
            DiskPercent = dto.DiskPercent,
            IsReachable = dto.IsReachable,
            MeasuredAt = DateTime.UtcNow
        });

        var problems = new List<(string Title, string Priority, string Text)>();
        if (!dto.IsReachable)
            problems.Add(("Geraet nicht erreichbar", "kritisch", "Das Geraet hat sich nicht gemeldet."));
        if (dto.DiskPercent >= 90)
            problems.Add(("Festplatte fast voll", "hoch", $"Belegung: {dto.DiskPercent} %"));
        if (dto.RamPercent >= 90)
            problems.Add(("Arbeitsspeicher knapp", "mittel", $"Belegung: {dto.RamPercent} %"));
        if (dto.CpuPercent >= 95)
            problems.Add(("CPU stark ausgelastet", "mittel", $"Auslastung: {dto.CpuPercent} %"));

        var newTickets = 0;
        foreach (var p in problems)
        {
            var exists = await _db.Tickets.AnyAsync(t =>
                t.DeviceId == dto.DeviceId && t.Source == "agent" && t.Title == p.Title
                && (t.Status == "offen" || t.Status == "in_bearbeitung"));
            if (exists) continue;

            _db.Tickets.Add(new Ticket
            {
                Title = p.Title,
                Description = p.Text,
                Priority = p.Priority,
                DeviceId = dto.DeviceId,
                Source = "agent"
            });
            newTickets++;
        }

        await _db.SaveChangesAsync();
        return Ok(new { stored = true, newTickets });
    }
}