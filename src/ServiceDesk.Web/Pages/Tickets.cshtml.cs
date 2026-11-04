using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDesk.Web.Services;

namespace ServiceDesk.Web.Pages;

public class TicketsModel : PageModel
{
    private readonly ApiClient _api;
    private Dictionary<int, string> _devicesNames = new();

    public TicketsModel(ApiClient api) => _api = api;

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Bitte einen Titel eingeben.")]
    [StringLength(150, ErrorMessage = "Der Titel darf höchstens 150 Zeichen lang sein.")]
    public string Title { get; set; } = "";

    [BindProperty]public string? Description { get; set; }
    [BindProperty]public string Priority { get; set; } = "mittel";
    [BindProperty]public int? DeviceId { get; set; }

    [TempData] public string? Message { get; set; }
    public string? Error { get; set; }

    public List<TicketDto> Tickets { get; private set; } = new();
    public List<SelectListItem> DeviceOptions { get; private set; } = new();

    private string Token => User.FindFirstValue("api_token") ?? "";

    public string DeviceName(int? id) =>
        id is null ? "-": _devicesNames.GetValueOrDefault(id.Value, "#" + id);

    public static string StatusLabel(string status) => status switch
    {
        "offen" => "Offen",
        "in_bearbeitung" => "In Bearbeitung",
        "geloest" => "Gelöst",
        "geschlossen" => "Geschlossen",
        _ => status
    };

    public static string PriorityClass(string priority) => priority switch
    {
        "kritisch" => "text-danger fw-bold",
        "hoch" => "text-warning fw-semibold",
        _ => ""
    };

    public async Task<IActionResult> OnGetAsync() => await LoadAsync() ?? Page();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return await LoadAsync() ?? Page();

        try
        {
            var error = await _api.CreateTicketAsync(
                Token, new CreateTicketRequest(Title.Trim(), Description, Priority, DeviceId));
            if (error is null)
            {
                Message = "Ticket angelegt.";
                return RedirectToPage(new { Status });
            }
            Error = error;
        }
        catch (ApiUnauthorizedException)
        {
            return await SignOutAsync();
        }
        catch (HttpRequestException ex)
        {
            Error = "API-Fehler: "+ ex.Message;
        }
        return await LoadAsync() ?? Page();
    }

    private async Task<IActionResult?> LoadAsync()
    {
        try
        {
            var devices = await _api.GetDevicesAsync(Token);
            _devicesNames = devices.ToDictionary(d => d.Id, d => d.Name);
            DeviceOptions = devices.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
            Tickets = await _api.GetTicketsAsync(Token, Status);
            return null;
        }
        catch (ApiUnauthorizedException)
        {
            return await SignOutAsync();
        }
        catch (HttpRequestException ex)
        {
            Error = "API-Fehler: " + ex.Message;
            return null;
        }
    }

    private async Task<IActionResult> SignOutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Login");
    }

}