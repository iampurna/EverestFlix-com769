using System.Net;
using System.Net.Http.Json;
using EverestFlix.Client.Models;

namespace EverestFlix.Client.Services;

public class EverestFlixApiClient
{
    private readonly HttpClient _http;

    public EverestFlixApiClient(HttpClient http) { _http = http; }

    // ---------- Auth ----------

    public async Task<(AuthResponse? Response, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var res = await _http.PostAsJsonAsync("api/auth/register", request);
        if (res.IsSuccessStatusCode)
            return (await res.Content.ReadFromJsonAsync<AuthResponse>(), null);

        return (null, await FriendlyError(res, "Registration failed."));
    }

    public async Task<(AuthResponse? Response, string? Error)> LoginAsync(LoginRequest request)
    {
        var res = await _http.PostAsJsonAsync("api/auth/login", request);
        if (res.IsSuccessStatusCode)
            return (await res.Content.ReadFromJsonAsync<AuthResponse>(), null);

        return (null, await FriendlyError(res, "Invalid email or password."));
    }

    public async Task<UserInfo?> GetMeAsync()
    {
        var res = await _http.GetAsync("api/auth/me");
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<UserInfo>() : null;
    }

    // ---------- Videos ----------

    public async Task<PagedResponse<VideoSummary>> GetLatestAsync(int page = 1, int pageSize = 10)
    {
        var res = await _http.GetFromJsonAsync<PagedResponse<VideoSummary>>(
            $"api/videos/latest?page={page}&pageSize={pageSize}");
        return res ?? new PagedResponse<VideoSummary>();
    }

    public async Task<PagedResponse<VideoSummary>> SearchAsync(string? q, string? genre = null, int page = 1, int pageSize = 12)
    {
        var url = $"api/videos/search?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(q))     url += $"&q={Uri.EscapeDataString(q)}";
        if (!string.IsNullOrWhiteSpace(genre)) url += $"&genre={Uri.EscapeDataString(genre)}";

        var res = await _http.GetFromJsonAsync<PagedResponse<VideoSummary>>(url);
        return res ?? new PagedResponse<VideoSummary>();
    }

    public async Task<VideoDetail?> GetVideoAsync(int id)
    {
        var res = await _http.GetAsync($"api/videos/{id}");
        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<VideoDetail>();
    }

    // ---------- helpers ----------

    private static async Task<string> FriendlyError(HttpResponseMessage res, string fallback)
    {
        try
        {
            var payload = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (payload is not null && payload.TryGetValue("errors", out var errs) && errs is not null)
                return errs.ToString() ?? fallback;
        }
        catch { /* swallow */ }
        return fallback;
    }
}