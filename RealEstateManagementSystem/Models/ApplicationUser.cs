using Microsoft.AspNetCore.Identity;

namespace RealEstateManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Properties owned/sold/rented out by this user.
        /// One user can own many properties - the same account can also buy or rent another
        /// user's property in a later phase, so there is no separate Seller or Buyer account.
        /// </summary>
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
