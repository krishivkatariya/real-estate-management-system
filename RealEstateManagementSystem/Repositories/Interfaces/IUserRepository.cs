using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.Repositories.Interfaces
{
    // Wait, the UserManager already acts as a repository for ApplicationUser.
    // However, the requirements specify to create an IUserRepository using the Repository Pattern 
    // where appropriate, and explain in comments where Identity managers are used directly 
    // and where repository abstraction is useful.

    /// <summary>
    /// User repository interface.
    /// Note: ASP.NET Core Identity's UserManager and SignInManager provide the primary abstractions
    /// for authentication and user management tasks (like creation, password hashing, and roles).
    /// This repository can be used for custom querying or extensions specific to the domain 
    /// that are not covered well by UserManager (e.g., specific reporting or batch updates).
    /// </summary>
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        // Add more domain-specific user functions here in the future
    }
}
