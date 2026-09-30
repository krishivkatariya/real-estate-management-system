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
    }
}
