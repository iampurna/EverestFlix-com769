using EverestFlix.Application.Common;
using EverestFlix.Application.DTOs.Comments;
using EverestFlix.Infrastructure.Comments;
using Microsoft.Extensions.Logging.Abstractions;

namespace EverestFlix.UnitTests;

public class CommentServiceTests
{
    [Fact]
    public async Task AddAsync_WhitespaceComment_ReturnsValidationError()
    {
        await using var db =
            TestDbFactory.Create();

        var (user, video) =
            await TestData.AddUserAndVideoAsync(
                db);

        var service =
            new CommentService(
                db,
                NullLogger<CommentService>.Instance);


        var result =
            await service.AddAsync(
                video.Id,
                new CreateCommentRequest
                {
                    Text = "       "
                },
                user.Id);


        Assert.False(
            result.Succeeded);

        Assert.Equal(
            ErrorCodes.ValidationError,
            result.ErrorCode);

        Assert.Empty(
            db.Comments);
    }


    [Fact]
    public async Task AddAsync_CommentOver1000Characters_ReturnsValidationError()
    {
        await using var db =
            TestDbFactory.Create();

        var (user, video) =
            await TestData.AddUserAndVideoAsync(
                db);

        var service =
            new CommentService(
                db,
                NullLogger<CommentService>.Instance);


        var result =
            await service.AddAsync(
                video.Id,
                new CreateCommentRequest
                {
                    Text = new string(
                        'x',
                        1001)
                },
                user.Id);


        Assert.False(
            result.Succeeded);

        Assert.Equal(
            ErrorCodes.ValidationError,
            result.ErrorCode);

        Assert.Empty(
            db.Comments);
    }


    [Fact]
    public async Task AddAsync_ValidComment_TrimsAndSavesText()
    {
        await using var db =
            TestDbFactory.Create();

        var (user, video) =
            await TestData.AddUserAndVideoAsync(
                db);

        var service =
            new CommentService(
                db,
                NullLogger<CommentService>.Instance);


        var result =
            await service.AddAsync(
                video.Id,
                new CreateCommentRequest
                {
                    Text =
                        "   EverestFlix works   "
                },
                user.Id);


        Assert.True(
            result.Succeeded);

        Assert.NotNull(
            result.Value);

        Assert.Equal(
            "EverestFlix works",
            result.Value!.Text);

        Assert.Single(
            db.Comments);
    }


    [Fact]
    public async Task AddAsync_MissingVideo_ReturnsNotFound()
    {
        await using var db =
            TestDbFactory.Create();

        var userId =
            Guid.NewGuid().ToString();

        var service =
            new CommentService(
                db,
                NullLogger<CommentService>.Instance);


        var result =
            await service.AddAsync(
                999,
                new CreateCommentRequest
                {
                    Text = "Hello"
                },
                userId);


        Assert.False(
            result.Succeeded);

        Assert.Equal(
            ErrorCodes.NotFound,
            result.ErrorCode);
    }
}