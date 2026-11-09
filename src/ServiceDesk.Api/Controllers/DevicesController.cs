using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;
using ServiceDesk.Api.Models;

namespace ServiceDesk.Api.Controllers;

public record CreateDeviceDto(string Name, string Type, string? Hostname, string? IpAddress, string? Location);

[ApiController]
[Authorize]
[Route("[controller]")]
public class DevicesController : ControllerBase
{
    private readonly ServiceDeskContext _db;

    public DevicesController(ServiceDeskContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Device>>> GetAll()
        => await _db.Devices.AsNoTracking().ToListAsync();

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<Device>> Create(CreateDeviceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Type))
            return BadRequest("Name und Typ sind Pflicht.");

        if (!string.IsNullOrWhiteSpace(dto.IpAddress) && !IPAddress.TryParse(dto.IpAddress.Trim(), out _))
            return BadRequest("Ungueltige IP-Adresse.");

        var device = new Device
        {
            Name = dto.Name.Trim(),
            Type = dto.Type.Trim(),
            Hostname = dto.Hostname,
            IpAddress = dto.IpAddress?.Trim(),
            Location = dto.Location
        };

        _db.Devices.Add(device);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), device);
    }
}