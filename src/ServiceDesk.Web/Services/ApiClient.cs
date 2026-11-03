using System.Net.Http.Json;
using System.Text.Json;

namespace ServiceDesk.Web.Services;

public record LoginResult(string Token, string Role);

public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    public ApiClient(HttpClient http) => _http = http;

    public async Task<LoginResult?> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new { username, password });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResult>(Json);
    }


}