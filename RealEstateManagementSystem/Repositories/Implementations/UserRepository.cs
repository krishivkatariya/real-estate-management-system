using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.Repositories.Interfaces;

namespace RealEstateManagementSystem.Repositories.Implementations
{
    /// <summary>
    /// Implementation of IUserRepository.
    /// Most authentication and user creation operations are handled directly by UserManager in AccountController.
    /// This repository provides domain-specific queries on users.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}
