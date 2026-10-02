using Microsoft.AspNetCore.Identity;

namespace realEstate.Models;

public class ApplicationUser : IdentityUser
{
    // Extend with additional profile fields later if needed
    public ICollection<RentalTransaction> RentalsAsRenter { get; set; } = new List<RentalTransaction>();
    public ICollection<RentalTransaction> RentalsAsOwner { get; set; } = new List<RentalTransaction>();
}
