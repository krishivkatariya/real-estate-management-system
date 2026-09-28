using Microsoft.AspNetCore.Identity;

namespace RealEstateManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
