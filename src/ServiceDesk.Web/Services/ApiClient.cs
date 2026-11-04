using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ServiceDesk.Web.Services;

public record LoginResult(string Token, string Role);
public record DeviceDto(int Id, string Name, string Type);
public record TicketDto(int Id, string Title, string? Description, string Status, string Priority,
    int? CreatedBy, int? AssignedTo, int? DeviceId, string Source, string? SolutionNote,
    DateTime CreatedAt, DateTime? ResolvedAt);
public record CreateTicketRequest(string Title, string? Description, string Priority, int? DeviceId);

public class ApiUnauthorizedException : Exception { }

public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    public async Task<LoginResult?> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new { username, password });
        if (response.StatusCode == HttpStatusCode.Unauthorized) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResult>(Json);
    }

    public async Task<List<DeviceDto>> GetDevicesAsync(string token)
    {
        var response = await SendAsync(HttpMethod.Get, "devices", token);
        return await response.Content.ReadFromJsonAsync<List<DeviceDto>>(Json) ?? new();
    }

    public async Task<List<TicketDto>> GetTicketsAsync(string token, string? status)
    {
        var url = string.IsNullOrEmpty(status) ? "tickets" : "tickets?status=" + Uri.EscapeDataString(status);
        var response = await SendAsync(HttpMethod.Get, url, token);
        return await response.Content.ReadFromJsonAsync<List<TicketDto>>(Json) ?? new();
    }

    // Gibt null bei Erfolg zurück, sonst die Fehlermeldung der API
    public async Task<string?> CreateTicketAsync(string token, CreateTicketRequest ticket)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "tickets");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(ticket, options: Json);

        var response = await _http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new ApiUnauthorizedException();
        if (response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string token)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new ApiUnauthorizedException();
        response.EnsureSuccessStatusCode();
        return response;
    }
}