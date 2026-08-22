using EverestFlix.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EverestFlix.Infrastructure.Data;

public class EverestFlixDbContext : IdentityDbContext<ApplicationUser>
{
    public EverestFlixDbContext(DbContextOptions<EverestFlixDbContext> options) : base(options) { }

    public DbSet<Video>   Videos   => Set<Video>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Rating>  Ratings  => Set<Rating>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Video>(v =>
        {
            v.Property(x => x.Title).IsRequired().HasMaxLength(200);
            v.Property(x => x.Description).HasMaxLength(2000);
            v.Property(x => x.Publisher).IsRequired().HasMaxLength(150);
            v.Property(x => x.Producer).IsRequired().HasMaxLength(150);
            v.Property(x => x.Genre).IsRequired().HasMaxLength(80);
            v.Property(x => x.VideoUrl).IsRequired().HasMaxLength(500);
            v.Property(x => x.ThumbnailUrl).HasMaxLength(500);

            v.HasOne(x => x.Creator)
             .WithMany(u => u.Videos)
             .HasForeignKey(x => x.CreatorId)
             .OnDelete(DeleteBehavior.Restrict);

            v.HasIndex(x => x.CreatedAt);
            v.HasIndex(x => x.Genre);
            v.HasIndex(x => x.CreatorId);
        });

        builder.Entity<Comment>(c =>
        {
            c.Property(x => x.Text).IsRequired().HasMaxLength(1000);

            c.HasOne(x => x.Video)
             .WithMany(v => v.Comments)
             .HasForeignKey(x => x.VideoId)
             .OnDelete(DeleteBehavior.Cascade);

            c.HasOne(x => x.User)
             .WithMany(u => u.Comments)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            c.HasIndex(x => x.VideoId);
        });

        builder.Entity<Rating>(r =>
        {
            r.HasOne(x => x.Video)
             .WithMany(v => v.Ratings)
             .HasForeignKey(x => x.VideoId)
             .OnDelete(DeleteBehavior.Cascade);

            r.HasOne(x => x.User)
             .WithMany(u => u.Ratings)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            // Enforces "one rating per user per video" at DB level
            r.HasIndex(x => new { x.VideoId, x.UserId }).IsUnique();
        });
    }
}