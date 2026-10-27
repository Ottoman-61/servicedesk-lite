using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;
using ServiceDesk.Api.Models;

namespace ServiceDesk.Api.Controllers;

[ApiController]
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
    public async Task<ActionResult<Ticket>> Create(CreateTicketDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return  BadRequest("Titel fehlt.");


        var priority = dto.Priority ?? "mittel";
        if (!Priorities.Contains(priority)) return BadRequest("Ungueltige Prioritaet.");

        if(dto.CreatedBy is not null && !await _db.Users.AnyAsync(u => u.Id == dto.CreatedBy))
            return BadRequest("Ersteller existiert nicht.");
        
        if(dto.DeviceId is not null && !await _db.Devices.AnyAsync(d => d.Id == dto.DeviceId))
            return BadRequest("Geraet existiert nicht.");

        var ticket = new Ticket
        {
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Priority = priority,
            CreatedBy = dto.CreatedBy,
            DeviceId = dto.DeviceId,
        };
        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = ticket.Id }, ticket);
    }

    [HttpPatch("{id}")]
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