using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.Repositories.Interfaces;

namespace RealEstateManagementSystem.Repositories.Implementations
{
    /// <summary>EF Core implementation of <see cref="ILocationRepository"/>.</summary>
    public class LocationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _context;

        public LocationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await _context.Locations
                .AsNoTracking()
                .OrderBy(l => l.State).ThenBy(l => l.City).ThenBy(l => l.Pincode)
                .ToListAsync();
        }

        public async Task<IEnumerable<Location>> GetActiveAsync()
        {
            return await _context.Locations
                .AsNoTracking()
                .Where(l => l.IsActive)
                .OrderBy(l => l.State).ThenBy(l => l.City).ThenBy(l => l.Pincode)
                .ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int locationId)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(l => l.LocationId == locationId);
        }

        public async Task<bool> ExistsAsync(string city, string state, string pincode, int? excludeLocationId = null)
        {
            var query = _context.Locations.AsNoTracking()
                .Where(l => l.City == city && l.State == state && l.Pincode == pincode);

            if (excludeLocationId.HasValue)
            {
                query = query.Where(l => l.LocationId != excludeLocationId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasPropertiesAsync(int locationId)
        {
            return await _context.Properties
                .AsNoTracking()
                .AnyAsync(p => p.LocationId == locationId);
        }

        public async Task<int> CountPropertiesAsync(int locationId)
        {
            return await _context.Properties
                .AsNoTracking()
                .CountAsync(p => p.LocationId == locationId);
        }

        public async Task AddAsync(Location location)
        {
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Location location)
        {
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
