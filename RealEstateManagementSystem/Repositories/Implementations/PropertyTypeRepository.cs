using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.Repositories.Interfaces;

namespace RealEstateManagementSystem.Repositories.Implementations
{
    /// <summary>EF Core implementation of <see cref="IPropertyTypeRepository"/>.</summary>
    public class PropertyTypeRepository : IPropertyTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public PropertyTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PropertyType>> GetAllAsync()
        {
            return await _context.PropertyTypes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<PropertyType>> GetActiveAsync()
        {
            return await _context.PropertyTypes
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<PropertyType?> GetByIdAsync(int propertyTypeId)
        {
            return await _context.PropertyTypes
                .FirstOrDefaultAsync(t => t.PropertyTypeId == propertyTypeId);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludePropertyTypeId = null)
        {
            var query = _context.PropertyTypes.AsNoTracking()
                .Where(t => t.Name == name);

            if (excludePropertyTypeId.HasValue)
            {
                query = query.Where(t => t.PropertyTypeId != excludePropertyTypeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasPropertiesAsync(int propertyTypeId)
        {
            return await _context.Properties
                .AsNoTracking()
                .AnyAsync(p => p.PropertyTypeId == propertyTypeId);
        }

        public async Task<int> CountPropertiesAsync(int propertyTypeId)
        {
            return await _context.Properties
                .AsNoTracking()
                .CountAsync(p => p.PropertyTypeId == propertyTypeId);
        }

        public async Task AddAsync(PropertyType propertyType)
        {
            await _context.PropertyTypes.AddAsync(propertyType);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PropertyType propertyType)
        {
            _context.PropertyTypes.Update(propertyType);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
