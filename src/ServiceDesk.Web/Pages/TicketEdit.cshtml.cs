using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDesk.Web.Services;

namespace ServiceDesk.Web.Pages;

[Authorize(Roles = "techniker,admin")]
public class TicketEditModel : PageModel
{
    private readonly ApiClient _api;

    public TicketEditModel(ApiClient api) => _api = api;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    [BindProperty] public string Status { get; set; } = "offen";
    [BindProperty] public string Priority { get; set; } = "mittel";
    [BindProperty] public int? AssignedTo { get; set; }
    [BindProperty] public string? SolutionNote { get; set; }

    [TempData] public string? Message { get; set; }
    public string? Error { get; set; }

    public TicketDto? Ticket { get; private set; }
    public List<SelectListItem> UserOptions { get; private set; } = new();

    private string Token => User.FindFirstValue("api_token") ?? "";

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            if (!await LoadAsync()) return NotFound();
            Status = Ticket!.Status;
            Priority = Ticket.Piority;
            AssignedTo = Ticket.AssignedTo;
            SolutionNote = Ticket.SolutionNote;
            return Page();
        }
        catch (ApiUnauthorizedException)
        {
            return await SignOutAsync();
        }
        catch (HttpRequestException ex)
        {
            Error = "API-Fehler: " + ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var error = await _api.UpdateTicketAsync(
                Token, Id, new UpdateTicketRequest(Status, Priority, AssignedTo, SolutionNote));
            if (error is null)
            {
                Message = "Ticket gespeichert.";
                return RedirectToPage(new { id = Id });
            }

            Error = error;
            if (!await LoadAsync()) return NotFound();
            return Page();
        }
        catch (ApiUnauthorizedException)
        {
            return await SignOutAsync();
        }
        catch (HttpRequestException ex)
        {
            Error = "API-Fehler: " + ex.Message;
            return Page();
        }
    }

    private async Task<bool> LoadAsync()
    {
        Ticket = await _api.GetTicketAsync(Token, Id);
        if (Ticket is null) return false;

        var users = await _api.GetUsersAsync(Token);
        UserOptions = users
            .Select(u => new SelectListItem(u.Username + " (" + u.Role + ")", u.Id.ToString()))
            .ToList();
        return true;
    }

    private async Task<IActionResult> SignOutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }
}