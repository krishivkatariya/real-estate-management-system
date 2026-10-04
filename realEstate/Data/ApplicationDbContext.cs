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
    // Inquiries already present
    public DbSet<Agent> Agents { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; } = null!;
    public DbSet<RentalTransaction> RentalTransactions { get; set; } = null!;
    public DbSet<Inquiry> Inquiries { get; set; } = null!;

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

        modelBuilder.Entity<Inquiry>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Subject).HasMaxLength(200).IsRequired();
            entity.Property(i => i.Message).HasMaxLength(2000).IsRequired();
            entity.Property(i => i.ResponseMessage).HasMaxLength(2000);
            entity.Property(i => i.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(i => i.User).WithMany().HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(i => i.Property).WithMany().HasForeignKey(i => i.PropertyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(i => new { i.PropertyId, i.UserId });
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasKey(i => i.Id);
        });

        modelBuilder.Entity<Property>(entity =>
        {
            entity.Property(p => p.Balconies).HasDefaultValue(0);
            entity.Property(p => p.Pincode).HasMaxLength(20);
            entity.Property(p => p.Amenities).HasMaxLength(2000);
            entity.Property(p => p.RejectionReason).HasMaxLength(2000);
            entity.Property(p => p.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(p => p.ApprovalStatus);
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

        modelBuilder.Entity<RentalTransaction>(entity =>
        {
            entity.HasKey(rental => rental.RentalTransactionId);
            entity.Property(rental => rental.RentAmount).HasPrecision(18, 2);
            entity.Property(rental => rental.Notes).HasMaxLength(2000);
            entity.HasIndex(rental => new { rental.PropertyId, rental.Status, rental.StartDate, rental.EndDate });
            entity.HasIndex(rental => new { rental.PropertyId, rental.RenterId })
                .IsUnique()
                .HasDatabaseName("UX_RentalTransactions_PendingPropertyRenter")
                .HasFilter("[Status] = 0");
            entity.HasOne(rental => rental.Property)
                .WithMany(property => property.RentalTransactions)
                .HasForeignKey(rental => rental.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rental => rental.Renter)
                .WithMany(user => user.RentalsAsRenter)
                .HasForeignKey(rental => rental.RenterId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(rental => rental.Owner)
                .WithMany(user => user.RentalsAsOwner)
                .HasForeignKey(rental => rental.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
