using EverestFlix.Domain.Entities;
using EverestFlix.Domain.Enums;
using EverestFlix.Infrastructure.Data;

namespace EverestFlix.UnitTests;

internal static class TestData
{
    public static async Task<(ApplicationUser User, Video Video)>
        AddUserAndVideoAsync(
            EverestFlixDbContext db)
    {
        var user =
            new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "tester@everestflix.local",
                Email = "tester@everestflix.local",
                FullName = "Test User"
            };

        var video =
            new Video
            {
                Title = "Test Video",
                Description = "Unit test video",
                Publisher = "EverestFlix",
                Producer = "EverestFlix",
                Genre = "Testing",
                AgeRating = AgeRating.U,
                VideoUrl = "/uploads/videos/test.mp4",
                CreatorId = user.Id,
                Creator = user,
                CreatedAt = DateTime.UtcNow,
                IsPublished = true
            };

        db.Users.Add(user);
        db.Videos.Add(video);

        await db.SaveChangesAsync();

        return (
            user,
            video);
    }
}