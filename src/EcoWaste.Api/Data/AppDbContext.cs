using EcoWaste.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoWaste.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CollectionPoint> CollectionPoints => Set<CollectionPoint>();
    public DbSet<WasteRecord> WasteRecords => Set<WasteRecord>();
    public DbSet<CollectionAlert> CollectionAlerts => Set<CollectionAlert>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CollectionPoint>(e =>
        {
            e.ToTable("COLLECTION_POINTS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            e.Property(x => x.Address).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<WasteRecord>(e =>
        {
            e.ToTable("WASTE_RECORDS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).HasConversion<int>();
            e.Property(x => x.Description).HasMaxLength(300);
            e.HasOne(x => x.CollectionPoint)
                .WithMany(p => p.WasteRecords)
                .HasForeignKey(x => x.CollectionPointId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.RecordedAt);
            e.HasIndex(x => new { x.CollectionPointId, x.Collected });
        });

        modelBuilder.Entity<CollectionAlert>(e =>
        {
            e.ToTable("COLLECTION_ALERTS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Message).HasMaxLength(300);
            e.HasOne(x => x.CollectionPoint)
                .WithMany()
                .HasForeignKey(x => x.CollectionPointId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.Resolved);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("APP_USERS");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired().HasMaxLength(80);
            e.Property(x => x.Role).IsRequired().HasMaxLength(40);
            e.HasIndex(x => x.Username).IsUnique();
        });
    }
}
