using EverestFlix.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EverestFlix.UnitTests;

internal static class TestDbFactory
{
    public static EverestFlixDbContext Create()
    {
        var options =
            new DbContextOptionsBuilder<EverestFlixDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new EverestFlixDbContext(options);
    }
}