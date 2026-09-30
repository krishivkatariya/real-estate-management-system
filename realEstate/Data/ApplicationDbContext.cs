using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using realEstate.Models;

namespace realEstate.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties { get; set; } = null!;
    public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
    public DbSet<Agent> Agents { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.Area).HasColumnType("decimal(10,2)");
            entity.HasMany(p => p.Images)
                  .WithOne(i => i.Property)
                  .HasForeignKey(i => i.PropertyId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(p => p.Owner)
                  .WithMany()
                  .HasForeignKey(p => p.OwnerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(i => i.Id);
        });

        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(a => a.Id);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => new { f.UserId, f.PropertyId }).IsUnique();
            entity.HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(f => f.Property).WithMany().HasForeignKey(f => f.PropertyId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseRequest>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.Property).WithMany().HasForeignKey(r => r.PropertyId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(r => r.Message).HasMaxLength(2000);
            entity.Property(r => r.RequestDate).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
