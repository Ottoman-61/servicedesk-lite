using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceDesk.Web.Services;

namespace ServiceDesk.Web.Pages;

public class IndexModel : PageModel
{
    private static readonly CultureInfo De = new("de-DE");

    private readonly ApiClient _api;

    public IndexModel(ApiClient api) => _api = api;

    public DashboardDto? Dashboard { get; private set; }
    public string? Error { get; private set; }

    private string Token => User.FindFirstValue("api_token") ?? "";

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Dashboard = await _api.GetDashboardAsync(Token);
        }
        catch (ApiUnauthorizedException)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login");
        }
        catch (HttpRequestException ex)
        {
            Error = "API-Fehler: " + ex.Message;
        }
        return Page();
    }

    public static string Pct(decimal? value)
        => value is null ? "–" : value.Value.ToString("0.0", De) + " %";

    public static string LoadClass(decimal? value) => value switch
    {
        null => "",
        >= 90m => "table-danger",
        >= 75m => "table-warning",
        _ => ""
    };

    public static bool IsStale(DateTime? measuredAt)
        => measuredAt is null
           || DateTime.UtcNow - DateTime.SpecifyKind(measuredAt.Value, DateTimeKind.Utc) > TimeSpan.FromMinutes(10);

    public static string Seen(DateTime? measuredAt)
        => measuredAt is null
            ? "Noch nie"
            : DateTime.SpecifyKind(measuredAt.Value, DateTimeKind.Utc).ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss", De);
}