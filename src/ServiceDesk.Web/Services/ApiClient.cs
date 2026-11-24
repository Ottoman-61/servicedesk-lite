using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ServiceDesk.Web.Services;

public record LoginResult(string Token, string Role);
public record UserDto(int Id, string Username, string Role);
public record DeviceDto(int Id, string Name, string Type, string? Hostname, string IpAddress,  string? Location, DateOnly? WarrantyUntill, string Status);
public record TicketDto(int Id, string Title, string? Description, string Status, string Priority, int? CreatedBy, int? AssignedTo, int? DeviceId, string Source, string? SolutionNote, DateTime CreatedAt, DateTime? ResolvedAt);
public record CreateTicketRequest(string Title, string? Description, string Priority, int? DeviceId);
public record UpdateTicketRequest(string? Status, string? Priority, int? AssignedTo, string? SolutionNote);
public record CreateDeviceRequest(string Name, string Type, string? Hostname, string? IpAddress, string? Location);

public record  DeviceStatusDto(int DeviceId, string Name, string Type, string Status, decimal? CpuPercent, decimal? RamPercent, decimal? DiskPercent, bool? IsReachable, DateTime? MeasuredAt);
public record DashboardDto(int OpenTickets, int InProgressTickets, int CriticalOpenTickets, int AgentTickets, int ResolvedLast7Days, List<DeviceStatusDto> Devices);

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

    public async Task<List<UserDto>> GetUsersAsync(string token)
    {
        var response = await SendAsync(HttpMethod.Get, "users", token);
        return await response.Content.ReadFromJsonAsync<List<UserDto>>(Json) ?? new();
    }

    public async Task<List<TicketDto>> GetTicketsAsync(string token, string? status)
    {
        var url = string.IsNullOrEmpty(status) ? "tickets" : "ticktes?status=" + Uri.EscapeDataString(status);
        var response = await SendAsync(HttpMethod.Get, url, token);
        return await response.Content.ReadFromJsonAsync<List<TicketDto>>(Json) ?? new();
    }

    public async Task<TicketDto?> GetTicketAsync(string token, int id)
    {
        using var request = NewRequest(HttpMethod.Get, "tickets/" + id, token);
        var  response = await _http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new ApiUnauthorizedException();
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TicketDto>(Json);
    }

    public async Task<DashboardDto?> GetDashboardAsync(string token)
    {
        var response = await SendAsync(HttpMethod.Get, "dashboard", token);
        return await response.Content.ReadFromJsonAsync<DashboardDto>(Json);
    }

    // Bei diesen 3 Methoden bekommt man ei null wert wenn man Erfolg hat ansonsten Fehlermeldung von Api,
    public Task<string?> CreateTicketAsync(string token, CreateTicketRequest ticket)
        => WriteAsync(HttpMethod.Post, "tickets", token, ticket);
    
    public Task<string?> UpdateTicketAsync(string token, int id, UpdateTicketRequest ticket)
        => WriteAsync(HttpMethod.Patch, "tickets/" + id, token, ticket);

    public Task<string?> CreateDeviceAsync(string token, CreateDeviceRequest device)
        => WriteAsync(HttpMethod.Post, "devices", token, device);

        private async Task<string?> WriteAsync<T>(HttpMethod method, string url, string token, T body)
    {
        using var request = NewRequest(method, url, token);
        request.Content = JsonContent.Create(body, options: Json);

        var response = await _http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new ApiUnauthorizedException();
        if (response.StatusCode == HttpStatusCode.Forbidden) return "Dafür fehlt die nötige Rolle.";
        if (response.IsSuccessStatusCode) return null;

        var text = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(text)
            ? $"API-Antwort {(int)response.StatusCode} {response.ReasonPhrase}"
            : text;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string token)
    {
        using var request = NewRequest(method, url, token);
        var response = await _http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized) throw new ApiUnauthorizedException();
        response.EnsureSuccessStatusCode();
        return response;
    }

    private static HttpRequestMessage NewRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}