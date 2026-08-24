using EverestFlix.Application.Common;
using EverestFlix.Infrastructure.Ratings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EverestFlix.UnitTests;

public class RatingServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task SetAsync_InvalidRating_ReturnsValidationError(
        int value)
    {
        await using var db =
            TestDbFactory.Create();

        var (user, video) =
            await TestData.AddUserAndVideoAsync(
                db);

        var service =
            new RatingService(
                db,
                NullLogger<RatingService>.Instance);


        var result =
            await service.SetAsync(
                video.Id,
                value,
                user.Id);


        Assert.False(
            result.Succeeded);

        Assert.Equal(
            ErrorCodes.ValidationError,
            result.ErrorCode);

        Assert.Empty(
            db.Ratings);
    }


    [Fact]
    public async Task SetAsync_MissingVideo_ReturnsNotFound()
    {
        await using var db =
            TestDbFactory.Create();

        var userId =
            Guid.NewGuid().ToString();

        var service =
            new RatingService(
                db,
                NullLogger<RatingService>.Instance);


        var result =
            await service.SetAsync(
                999,
                5,
                userId);


        Assert.False(
            result.Succeeded);

        Assert.Equal(
            ErrorCodes.NotFound,
            result.ErrorCode);
    }


    [Fact]
    public async Task SetAsync_ValidRating_CreatesRating()
    {
        await using var db =
            TestDbFactory.Create();

        var (user, video) =
            await TestData.AddUserAndVideoAsync(
                db);

        var service =
            new RatingService(
                db,
                NullLogger<RatingService>.Instance);


        var result =
            await service.SetAsync(
                video.Id,
                4,
                user.Id);


        Assert.True(
            result.Succeeded);

        Assert.Equal(
            4,
            result.Value);


        var stored =
            await db.Ratings.SingleAsync();


        Assert.Equal(
            4,
            stored.Value);

        Assert.Equal(
            user.Id,
            stored.UserId);

        Assert.Equal(
            video.Id,
            stored.VideoId);
    }


    [Fact]
    public async Task SetAsync_SameUserRatesAgain_UpdatesExistingRating()
    {
        await using var db =
            TestDbFactory.Create();

        var (user, video) =
            await TestData.AddUserAndVideoAsync(
                db);

        var service =
            new RatingService(
                db,
                NullLogger<RatingService>.Instance);


        var first =
            await service.SetAsync(
                video.Id,
                2,
                user.Id);


        var second =
            await service.SetAsync(
                video.Id,
                5,
                user.Id);


        Assert.True(
            first.Succeeded);

        Assert.True(
            second.Succeeded);


        Assert.Equal(
            1,
            await db.Ratings.CountAsync());


        var stored =
            await db.Ratings.SingleAsync();


        Assert.Equal(
            5,
            stored.Value);
    }


    [Fact]
    public async Task GetSummaryAsync_ReturnsAverageCountAndMyRating()
    {
        await using var db =
            TestDbFactory.Create();

        var (user, video) =
            await TestData.AddUserAndVideoAsync(
                db);


        var secondUser =
            new EverestFlix.Domain.Entities.ApplicationUser
            {
                Id =
                    Guid.NewGuid().ToString(),

                UserName =
                    "second@everestflix.local",

                Email =
                    "second@everestflix.local",

                FullName =
                    "Second User"
            };


        db.Users.Add(
            secondUser);

        await db.SaveChangesAsync();


        var service =
            new RatingService(
                db,
                NullLogger<RatingService>.Instance);


        await service.SetAsync(
            video.Id,
            4,
            user.Id);


        await service.SetAsync(
            video.Id,
            2,
            secondUser.Id);


        var summary =
            await service.GetSummaryAsync(
                video.Id,
                user.Id);


        Assert.Equal(
            2,
            summary.Count);

        Assert.Equal(
            3.0,
            summary.Average);

        Assert.Equal(
            4,
            summary.MyRating);
    }
}