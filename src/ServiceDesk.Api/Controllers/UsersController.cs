using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Api.Data;

namespace ServiceDesk.Api.Controllers;

public record UserSummary(int Id, string Username, string Role);

[ApiController]
[Authorize(Roles = "techniker, admin")]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly ServiceDeskContext _db;

    public UsersController(ServiceDeskContext db) => _db = db;

    [HttpGet]
    public async   Task<ActionResult<List<UserSummary>>> GetAll()
        => await _db.Users.AsNoTracking()
            .Where(u => u.Role == "techniker" || u.Role == "admin")
            .OrderBy(u => u.Username)
            .Select(u => new UserSummary(u.Id, u.Username, u.Role))
            .ToListAsync();

}