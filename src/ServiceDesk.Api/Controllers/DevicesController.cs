using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;
using ServiceDesk.Api.Models;

namespace ServiceDesk.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DevicesController : ControllerBase
{
    private readonly ServiceDeskContext _db;

    public DevicesController(ServiceDeskContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Device>>> GetAll()
        => await _db.Devices.AsNoTracking().ToListAsync();
}