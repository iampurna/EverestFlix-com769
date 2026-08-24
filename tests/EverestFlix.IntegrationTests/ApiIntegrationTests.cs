using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Auth;
using EverestFlix.Application.DTOs.Comments;
using EverestFlix.Application.DTOs.Ratings;
using EverestFlix.Application.DTOs.Videos;
using EverestFlix.Domain.Enums;

namespace EverestFlix.IntegrationTests;


public class ApiIntegrationTests
    : IClassFixture<EverestFlixApiFactory>
{
    private readonly EverestFlixApiFactory _factory;


    public ApiIntegrationTests(
        EverestFlixApiFactory factory)
    {
        _factory =
            factory;
    }


    // -------------------------------------------------------------
    // Authentication
    // -------------------------------------------------------------


    [Fact]
    public async Task Register_CreatesConsumerAndReturnsToken()
    {
        using var client =
            _factory.CreateClient();


        var request =
            CreateUniqueConsumer();


        var response =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                request);


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);


        var auth =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();


        Assert.NotNull(
            auth);

        Assert.False(
            string.IsNullOrWhiteSpace(
                auth!.Token));

        Assert.Equal(
            request.Email,
            auth.User.Email);

        Assert.Contains(
            "Consumer",
            auth.User.Roles);
    }


    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        using var client =
            _factory.CreateClient();


        var request =
            CreateUniqueConsumer();


        var first =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                request);


        Assert.Equal(
            HttpStatusCode.OK,
            first.StatusCode);


        var second =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                request);


        Assert.Equal(
            HttpStatusCode.Conflict,
            second.StatusCode);
    }


    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();


        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email =
                        EverestFlixApiFactory.CreatorEmail,

                    Password =
                        "DefinitelyWrongPassword1"
                });


        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }


    [Fact]
    public async Task Me_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client =
            _factory.CreateClient();


        var response =
            await client.GetAsync(
                "/api/auth/me");


        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }


    // -------------------------------------------------------------
    // Video authorization / upload
    // -------------------------------------------------------------


    [Fact]
    public async Task Consumer_CannotUploadVideo()
    {
        using var client =
            _factory.CreateClient();


        var consumer =
            await RegisterConsumerAsync(
                client);


        SetBearer(
            client,
            consumer.Token);


        using var content =
            CreateVideoUploadContent(
                $"Consumer Upload {Guid.NewGuid():N}");


        var response =
            await client.PostAsync(
                "/api/videos",
                content);


        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }


    [Fact]
    public async Task Creator_CanUploadVideo()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var title =
            $"Integration Upload {Guid.NewGuid():N}";


        using var content =
            CreateVideoUploadContent(
                title);


        var response =
            await client.PostAsync(
                "/api/videos",
                content);


        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);


        var video =
            await response.Content
                .ReadFromJsonAsync<VideoDetailDto>();


        Assert.NotNull(
            video);

        Assert.Equal(
            title,
            video!.Title);

        Assert.EndsWith(
            ".mp4",
            video.VideoUrl,
            StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public async Task Search_FindsUploadedVideo()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var uniqueTerm =
            $"EverestSearch{Guid.NewGuid():N}";


        var uploaded =
            await UploadVideoAsync(
                client,
                uniqueTerm);


        // Search is public.
        client.DefaultRequestHeaders.Authorization =
            null;


        var result =
            await client.GetFromJsonAsync<
                PagedResponse<VideoSummaryDto>>(
                $"/api/videos/search?q={uniqueTerm}");


        Assert.NotNull(
            result);

        Assert.Contains(
            result!.Items,
            v => v.Id == uploaded.Id);
    }


    // -------------------------------------------------------------
    // Comments
    // -------------------------------------------------------------


    [Fact]
    public async Task AuthenticatedConsumer_CanCommentOnVideo()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var video =
            await UploadVideoAsync(
                client,
                $"Comment Test {Guid.NewGuid():N}");


        var consumer =
            await RegisterConsumerAsync(
                client);


        SetBearer(
            client,
            consumer.Token);


        var response =
            await client.PostAsJsonAsync(
                $"/api/videos/{video.Id}/comments",
                new CreateCommentRequest
                {
                    Text =
                        "Integration comment"
                });


        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);


        var comment =
            await response.Content
                .ReadFromJsonAsync<CommentDto>();


        Assert.NotNull(
            comment);

        Assert.Equal(
            "Integration comment",
            comment!.Text);
    }


    [Fact]
    public async Task WhitespaceComment_ReturnsBadRequest()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var video =
            await UploadVideoAsync(
                client,
                $"Whitespace Test {Guid.NewGuid():N}");


        var consumer =
            await RegisterConsumerAsync(
                client);


        SetBearer(
            client,
            consumer.Token);


        var response =
            await client.PostAsJsonAsync(
                $"/api/videos/{video.Id}/comments",
                new CreateCommentRequest
                {
                    Text =
                        "       "
                });


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    // -------------------------------------------------------------
    // Ratings
    // -------------------------------------------------------------


    [Fact]
    public async Task InvalidRating_ReturnsBadRequest()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var video =
            await UploadVideoAsync(
                client,
                $"Rating Validation {Guid.NewGuid():N}");


        var consumer =
            await RegisterConsumerAsync(
                client);


        SetBearer(
            client,
            consumer.Token);


        var response =
            await client.PostAsJsonAsync(
                $"/api/videos/{video.Id}/rating",
                new SetRatingRequest
                {
                    Value =
                        6
                });


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    [Fact]
    public async Task RatingSameVideoAgain_UpdatesInsteadOfDuplicating()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var video =
            await UploadVideoAsync(
                client,
                $"Rating Update {Guid.NewGuid():N}");


        var consumer =
            await RegisterConsumerAsync(
                client);


        SetBearer(
            client,
            consumer.Token);


        var first =
            await client.PostAsJsonAsync(
                $"/api/videos/{video.Id}/rating",
                new SetRatingRequest
                {
                    Value =
                        2
                });


        Assert.Equal(
            HttpStatusCode.OK,
            first.StatusCode);


        var second =
            await client.PostAsJsonAsync(
                $"/api/videos/{video.Id}/rating",
                new SetRatingRequest
                {
                    Value =
                        5
                });


        Assert.Equal(
            HttpStatusCode.OK,
            second.StatusCode);


        var summary =
            await client.GetFromJsonAsync<
                RatingSummaryDto>(
                $"/api/videos/{video.Id}/rating");


        Assert.NotNull(
            summary);


        Assert.Equal(
            1,
            summary!.Count);

        Assert.Equal(
            5.0,
            summary.Average);

        Assert.Equal(
            5,
            summary.MyRating);
    }


    // -------------------------------------------------------------
    // Editing / authorization
    // -------------------------------------------------------------


    [Fact]
    public async Task Creator_CanUpdateOwnVideo()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var video =
            await UploadVideoAsync(
                client,
                $"Original Title {Guid.NewGuid():N}");


        var updatedTitle =
            $"Updated Title {Guid.NewGuid():N}";


        var response =
            await client.PutAsJsonAsync(
                $"/api/videos/{video.Id}",
                new UpdateVideoRequest
                {
                    Title =
                        updatedTitle,

                    Description =
                        "Updated through integration test",

                    Publisher =
                        "EverestFlix",

                    Producer =
                        "Integration Tests",

                    Genre =
                        "Testing",

                    AgeRating =
                        AgeRating.PG
                });


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);


        var updated =
            await response.Content
                .ReadFromJsonAsync<VideoDetailDto>();


        Assert.NotNull(
            updated);

        Assert.Equal(
            updatedTitle,
            updated!.Title);
    }


    [Fact]
    public async Task Consumer_CannotUpdateCreatorVideo()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var video =
            await UploadVideoAsync(
                client,
                $"Protected Video {Guid.NewGuid():N}");


        var consumer =
            await RegisterConsumerAsync(
                client);


        SetBearer(
            client,
            consumer.Token);


        var response =
            await client.PutAsJsonAsync(
                $"/api/videos/{video.Id}",
                new UpdateVideoRequest
                {
                    Title =
                        "Unauthorized Update",

                    Publisher =
                        "Test",

                    Producer =
                        "Test",

                    Genre =
                        "Testing",

                    AgeRating =
                        AgeRating.U
                });


        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }


    // -------------------------------------------------------------
    // Delete / not found
    // -------------------------------------------------------------


    [Fact]
    public async Task Creator_CanDeleteOwnVideo()
    {
        using var client =
            _factory.CreateClient();


        var creator =
            await LoginCreatorAsync(
                client);


        SetBearer(
            client,
            creator.Token);


        var video =
            await UploadVideoAsync(
                client,
                $"Delete Test {Guid.NewGuid():N}");


        var deleteResponse =
            await client.DeleteAsync(
                $"/api/videos/{video.Id}");


        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);


        var getResponse =
            await client.GetAsync(
                $"/api/videos/{video.Id}");


        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }


    [Fact]
    public async Task MissingVideo_ReturnsNotFound()
    {
        using var client =
            _factory.CreateClient();


        var response =
            await client.GetAsync(
                "/api/videos/999999999");


        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }


    // -------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------


    private static RegisterRequest CreateUniqueConsumer()
    {
        return new RegisterRequest
        {
            FullName =
                "Integration Consumer",

            Email =
                $"consumer-{Guid.NewGuid():N}@everestflix.local",

            Password =
                "ConsumerTest#2026"
        };
    }


    private static async Task<AuthResponse>
        RegisterConsumerAsync(
            HttpClient client)
    {
        var request =
            CreateUniqueConsumer();


        var response =
            await client.PostAsJsonAsync(
                "/api/auth/register",
                request);


        response.EnsureSuccessStatusCode();


        var auth =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();


        return auth
            ?? throw new InvalidOperationException(
                "Consumer registration returned no auth response.");
    }


    private static async Task<AuthResponse>
        LoginCreatorAsync(
            HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            null;


        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest
                {
                    Email =
                        EverestFlixApiFactory.CreatorEmail,

                    Password =
                        EverestFlixApiFactory.CreatorPassword
                });


        response.EnsureSuccessStatusCode();


        var auth =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();


        return auth
            ?? throw new InvalidOperationException(
                "Creator login returned no auth response.");
    }


    private static void SetBearer(
        HttpClient client,
        string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }


    private static MultipartFormDataContent
        CreateVideoUploadContent(
            string title)
    {
        var content =
            new MultipartFormDataContent();


        content.Add(
            new StringContent(title),
            "title");


        content.Add(
            new StringContent(
                "Integration test video"),
            "description");


        content.Add(
            new StringContent(
                "EverestFlix"),
            "publisher");


        content.Add(
            new StringContent(
                "Integration Tests"),
            "producer");


        content.Add(
            new StringContent(
                "Testing"),
            "genre");


        content.Add(
            new StringContent(
                ((int)AgeRating.U).ToString()),
            "ageRating");


        // The storage layer currently validates MP4 extension,
        // MIME type and size. These bytes are sufficient for
        // testing the HTTP/storage workflow without shipping
        // a real media file inside the test project.
        var file =
            new ByteArrayContent(
                new byte[]
                {
                    0x00,
                    0x00,
                    0x00,
                    0x18,
                    0x66,
                    0x74,
                    0x79,
                    0x70,
                    0x6D,
                    0x70,
                    0x34,
                    0x32
                });


        file.Headers.ContentType =
            new MediaTypeHeaderValue(
                "video/mp4");


        content.Add(
            file,
            "videoFile",
            "integration-test.mp4");


        return content;
    }


    private static async Task<VideoDetailDto>
        UploadVideoAsync(
            HttpClient client,
            string title)
    {
        using var content =
            CreateVideoUploadContent(
                title);


        var response =
            await client.PostAsync(
                "/api/videos",
                content);


        response.EnsureSuccessStatusCode();


        var video =
            await response.Content
                .ReadFromJsonAsync<VideoDetailDto>();


        return video
            ?? throw new InvalidOperationException(
                "Upload returned no video.");
    }
}