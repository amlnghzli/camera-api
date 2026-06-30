using CameraApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CameraApi.Data;

public class CameraDbContext : DbContext
{
    public CameraDbContext(DbContextOptions<CameraDbContext> options) : base(options)
    {
    }

    public DbSet<Camera> Cameras => Set<Camera>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Camera entity
        modelBuilder.Entity<Camera>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Brand).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Model).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Sensor).HasMaxLength(200);
            entity.Property(e => e.Resolution).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(10, 2);
        });
    }
}
