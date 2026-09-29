using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ---------------- Phase 2: Property Management ----------------
        public DbSet<Property> Properties => Set<Property>();
        public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
        public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
        public DbSet<Location> Locations => Set<Location>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureProperty(builder);
            ConfigurePropertyImage(builder);
            ConfigurePropertyType(builder);
            ConfigureLocation(builder);
        }

        /// <summary>
        /// ApplicationUser (1) --&gt; (many) Property via Property.OwnerId.
        /// DeleteBehaviour.Restrict so that deleting a user can never silently cascade into
        /// property (and later transaction/history) data. The admin must deal with the
        /// listings explicitly instead.
        /// </summary>
        private static void ConfigureProperty(ModelBuilder builder)
        {
            builder.Entity<Property>(entity =>
            {
                entity.HasKey(p => p.PropertyId);

                entity.Property(p => p.Title)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(p => p.Description)
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Area)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Address)
                    .HasMaxLength(300)
                    .IsRequired();

                entity.Property(p => p.RejectionReason)
                    .HasMaxLength(500);

                // Store the enums as int so the database stays readable and stable.
                entity.Property(p => p.ListingType)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(p => p.Status)
                    .HasConversion<int>()
                    .IsRequired();

                entity.Property(p => p.OwnerId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(p => p.Owner)
                    .WithMany(u => u.Properties)
                    .HasForeignKey(p => p.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.PropertyType)
                    .WithMany(t => t.Properties)
                    .HasForeignKey(p => p.PropertyTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Location)
                    .WithMany(l => l.Properties)
                    .HasForeignKey(p => p.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Helpful indexes for the public listing, the owner's list and the admin queue.
                entity.HasIndex(p => p.Status);
                entity.HasIndex(p => p.OwnerId);
                entity.HasIndex(p => p.ListingType);
            });
        }

        /// <summary>
        /// Property (1) --&gt; (many) PropertyImage.
        /// Cascade is safe and expected here: removing a listing also removes its image rows.
        /// </summary>
        private static void ConfigurePropertyImage(ModelBuilder builder)
        {
            builder.Entity<PropertyImage>(entity =>
            {
                entity.HasKey(i => i.PropertyImageId);

                entity.Property(i => i.ImageUrl)
                    .HasMaxLength(300)
                    .IsRequired();

                entity.HasOne(i => i.Property)
                    .WithMany(p => p.Images)
                    .HasForeignKey(i => i.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(i => new { i.PropertyId, i.IsPrimary });
            });
        }

        private static void ConfigurePropertyType(ModelBuilder builder)
        {
            builder.Entity<PropertyType>(entity =>
            {
                entity.HasKey(t => t.PropertyTypeId);

                entity.Property(t => t.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(t => t.Description)
                    .HasMaxLength(250);

                entity.HasIndex(t => t.Name).IsUnique();
            });
        }

        private static void ConfigureLocation(ModelBuilder builder)
        {
            builder.Entity<Location>(entity =>
            {
                entity.HasKey(l => l.LocationId);

                entity.Property(l => l.City)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(l => l.State)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(l => l.Country)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(l => l.Pincode)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.HasIndex(l => new { l.City, l.State, l.Pincode }).IsUnique();
            });
        }
    }
}

