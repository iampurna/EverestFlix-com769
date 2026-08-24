using System.Net;
using System.Net.Http.Headers;
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
public async Task<long?> RecordViewAsync(
    int videoId)
{
    var res = await _http.PostAsync(
        $"api/videos/{videoId}/view",
        content: null);

    if (!res.IsSuccessStatusCode)
        return null;

    var payload =
        await res.Content
            .ReadFromJsonAsync<ViewCountResponse>();

    return payload?.ViewCount;
}
    public async Task<(VideoDetail? Response, string? Error)> UploadVideoAsync(
        string title, string? description, string publisher, string producer, string genre,
        int ageRating, Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(title),             "title");
        if (!string.IsNullOrEmpty(description))
            content.Add(new StringContent(description),   "description");
        content.Add(new StringContent(publisher),         "publisher");
        content.Add(new StringContent(producer),          "producer");
        content.Add(new StringContent(genre),             "genre");
        content.Add(new StringContent(ageRating.ToString()), "ageRating");

        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "videoFile", fileName);

        var res = await _http.PostAsync("api/videos", content);
        if (res.IsSuccessStatusCode)
            return (await res.Content.ReadFromJsonAsync<VideoDetail>(), null);

        return (null, await FriendlyError(res, "Upload failed."));
    }

    // ---------- Comments ----------

    public async Task<PagedResponse<CommentDto>> GetCommentsAsync(int videoId, int page = 1, int pageSize = 20)
    {
        var res = await _http.GetFromJsonAsync<PagedResponse<CommentDto>>(
            $"api/videos/{videoId}/comments?page={page}&pageSize={pageSize}");
        return res ?? new PagedResponse<CommentDto>();
    }

    public async Task<(CommentDto? Response, string? Error)> AddCommentAsync(int videoId, string text)
    {
        var res = await _http.PostAsJsonAsync($"api/videos/{videoId}/comments", new CreateCommentRequest { Text = text });
        if (res.IsSuccessStatusCode)
            return (await res.Content.ReadFromJsonAsync<CommentDto>(), null);
        return (null, await FriendlyError(res, "Could not post comment."));
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var res = await _http.DeleteAsync($"api/comments/{commentId}");
        return res.IsSuccessStatusCode;
    }

    // ---------- Ratings ----------

    public async Task<RatingSummary> GetRatingAsync(int videoId)
    {
        var res = await _http.GetFromJsonAsync<RatingSummary>($"api/videos/{videoId}/rating");
        return res ?? new RatingSummary();
    }

    public async Task<bool> SetRatingAsync(int videoId, int value)
    {
        var res = await _http.PostAsJsonAsync($"api/videos/{videoId}/rating", new SetRatingRequest { Value = value });
        return res.IsSuccessStatusCode;
    }

    // ---------- Creator ----------

    public async Task<PagedResponse<VideoSummary>> GetMyVideosAsync(int page = 1, int pageSize = 20)
    {
        var res = await _http.GetFromJsonAsync<PagedResponse<VideoSummary>>(
            $"api/creator/videos?page={page}&pageSize={pageSize}");
        return res ?? new PagedResponse<VideoSummary>();
    }

    public async Task<CreatorDashboardDto?> GetCreatorDashboardAsync()
    {
        var res = await _http.GetAsync("api/creator/dashboard");
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<CreatorDashboardDto>() : null;
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
        catch { }
        return fallback;
    }
}