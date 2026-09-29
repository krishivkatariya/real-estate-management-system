using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "User" };

            // Create roles if they don't exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user
            string adminEmail = configuration["AdminSettings:AdminEmail"] ?? "admin@realestate.com";
            string adminPassword = configuration["AdminSettings:AdminPassword"] ?? "Admin@12345";
            
            // Log info about using fallback credentials in development (avoid logging secrets in production)
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment() && string.IsNullOrEmpty(configuration["AdminSettings:AdminEmail"]))
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Using default fallback credentials for Admin seeding in Development.");
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true // Admin email is auto-confirmed
                };

                var createPowerUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Phase 2 - seed the lookup tables so the property form is usable right after setup.
            // Both seeders are idempotent: they only insert rows that are not there yet.
            await SeedPropertyTypesAsync(serviceProvider);
            await SeedLocationsAsync(serviceProvider);
        }

        /// <summary>Adds the default property types (Apartment, Villa, House...) when missing.</summary>
        private static async Task SeedPropertyTypesAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var defaultTypes = new (string Name, string Description)[]
            {
                ("Apartment", "Flat inside a multi-storey residential building."),
                ("Villa", "Independent luxury house with private open space."),
                ("Independent House", "Standalone house on its own plot."),
                ("Residential Plot", "Empty land meant for residential construction."),
                ("Commercial Land", "Land meant for commercial construction."),
                ("Office Space", "Space used for business or professional work."),
                ("Shop", "Retail space on a road or inside a market."),
                ("Farmhouse", "House located on agricultural or farm land.")
            };

            var existingNames = await context.PropertyTypes
                .Select(type => type.Name)
                .ToListAsync();

            var missing = defaultTypes
                .Where(type => !existingNames.Contains(type.Name))
                .Select(type => new PropertyType
                {
                    Name = type.Name,
                    Description = type.Description,
                    IsActive = true
                })
                .ToList();

            if (missing.Count > 0)
            {
                context.PropertyTypes.AddRange(missing);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>Adds a few default locations when the Location table is empty.</summary>
        private static async Task SeedLocationsAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (await context.Locations.AnyAsync())
            {
                return;
            }

            var locations = new List<Location>
            {
                new() { City = "Bengaluru", State = "Karnataka", Country = "India", Pincode = "560001", IsActive = true },
                new() { City = "Mysuru", State = "Karnataka", Country = "India", Pincode = "570001", IsActive = true },
                new() { City = "Hyderabad", State = "Telangana", Country = "India", Pincode = "500001", IsActive = true },
                new() { City = "Pune", State = "Maharashtra", Country = "India", Pincode = "411001", IsActive = true },
                new() { City = "Chennai", State = "Tamil Nadu", Country = "India", Pincode = "600001", IsActive = true }
            };

            context.Locations.AddRange(locations);
            await context.SaveChangesAsync();
        }
    }
}
