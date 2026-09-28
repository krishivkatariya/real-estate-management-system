using Microsoft.AspNetCore.Identity;
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
        }
    }
}
