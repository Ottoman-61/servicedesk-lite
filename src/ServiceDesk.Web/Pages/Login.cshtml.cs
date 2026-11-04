using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceDesk.Web.Services;

namespace ServiceDesk.Web.Pages;

public class LoginModel : PageModel
{
    private readonly ApiClient _api;

    public LoginModel(ApiClient api) => _api = api;

    [BindProperty] public string Username  { get; set; } = "";
    [BindProperty] public string Password  { get; set; } = "";
    public string? Error { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        LoginResult? result;
        try
        {
            result = await _api.LoginAsync(Username.Trim(), Password);
        }
        catch (HttpRequestException ex)
        {
            Error = "API nicht erreichbar oder Fehler: " + ex.Message;
            return Page();
        }

        if (result is null)
        {
            Error ="Benutzername oder Passwort ist falsch.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, Username.Trim()),
            new(ClaimTypes.Role, result.Role),
            new("api_token", result.Token)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToPage("/Index");
    }
}

