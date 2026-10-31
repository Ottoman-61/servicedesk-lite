using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;
using ServiceDesk.Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ServiceDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TicketsController : ControllerBase
{
    private static readonly string[] Statuses = ["offen", "in_bearbeitung", "geloest", "geschlossen"];
    private static readonly string[] Priorities =["niedrig", "mittel", "hoch", "kritisch"];

    private readonly ServiceDeskContext _db;

    public TicketsController(ServiceDeskContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Ticket>>> GetAll([FromQuery] string? status)
    {
        var query = _db.Tickets.AsNoTracking();
        if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status ==status);
        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Ticket>> GetOne(int id)
    {
        var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return ticket is null ? NotFound() : ticket;
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<Device>> Create(CreateDeviceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Type))
            return BadRequest("Name und Typ sind Pflicht.");


        var device = new Device
        {
            Name =dto.Name.Trim(),
            Type =dto.Type.Trim(),
            Hostname =dto.Hostname,
            IpAddress =dto.IpAddress,
            Location =dto.Location
        };
        _db.Devices.Add(device);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = device.Id }, device);

    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "techniker,admin")]
    public async Task<ActionResult<Ticket>> Update(int id, UpdateTicketDto dto)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();

        if (dto.Priority is not null)
        {
            if (!Priorities.Contains(dto.Priority)) return BadRequest("Ungueltige Prioritaet.");
            ticket.Priority = dto.Priority;
        }

        if (dto.AssignedTo is not null)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == dto.AssignedTo))
                return BadRequest("Bearbeiter existiert nicht.");
            ticket.AssignedTo = dto.AssignedTo;
        }

        if (dto.SolutionNote is not null) ticket.SolutionNote = dto.SolutionNote;

        if (dto.Status is not null)
        {
            if (!Statuses.Contains(dto.Status)) return BadRequest("Ungueltiger Status.");
            
            if (dto.Status is "geloest" or "geschlossen")
            {
              if (string.IsNullOrWhiteSpace(ticket.SolutionNote))
                  return BadRequest("Loesungsvermerk fehlt.");
                  ticket.ResolvedAt ??= DateTime.UtcNow;
            }
            else
            {
                ticket.ResolvedAt = null;
            }
            ticket.Status = dto.Status;
        }
        

        
        await _db.SaveChangesAsync();
        return ticket;
    }
}
public record CreateDeviceDto(string Name, string Type, string? Hostname, string? IpAddress, string? Location);