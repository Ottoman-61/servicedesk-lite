using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceDesk.Web.Services;

namespace ServiceDesk.Web.Pages;

public class DevicesModel : PageModel
{
    private readonly ApiClient _api;

    public DevicesModel(ApiClient api) => _api = api;

    [BindProperty]
    [Required(ErrorMessage = "Bitte einen Namen eingeben.")]
    public string Name { get; set; } = "";

    [BindProperty]
    [Required(ErrorMessage = "Bitte einen Typ eingeben.")]
    public string Type { get; set; } = "";

    [BindProperty] public string? Hostname { get; set; }
    [BindProperty] public string? IpAddress { get; set; }
    [BindProperty] public string? Location { get; set; }

    [TempData] public string? Message { get; set; }
    public string? Error { get; set; }

    public List<DeviceDto> Devices { get; private set; } = new();

    private string Token => User.FindFirstValue("api_token") ?? "";

    public async Task<IActionResult> OnGetAsync() => await LoadAsync() ?? Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.IsInRole("admin")) return Forbid();

        if (!string.IsNullOrWhiteSpace(IpAddress) && !IPAddress.TryParse(IpAddress.Trim(), out _))
            ModelState.AddModelError(nameof(IpAddress), "Das ist keine gültige IP-Adresse.");

        if (!ModelState.IsValid) return await LoadAsync() ?? Page();

        try
        {
            var error = await _api.CreateDeviceAsync(Token, new CreateDeviceRequest(
                Name.Trim(), Type.Trim(), Hostname, IpAddress?.Trim(), Location));
            if (error is null)
            {
                Message = "Gerät angelegt.";
                return RedirectToPage();
            }
            Error = error;
        }
        catch (ApiUnauthorizedException)
        {
            return await SignOutAsync();
        }
        catch (HttpRequestException ex)
        {
            Error = "API-Fehler: " + ex.Message;
        }

        return await LoadAsync() ?? Page();
    }

    private async Task<IActionResult?> LoadAsync()
    {
        try
        {
            Devices = await _api.GetDevicesAsync(Token);
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