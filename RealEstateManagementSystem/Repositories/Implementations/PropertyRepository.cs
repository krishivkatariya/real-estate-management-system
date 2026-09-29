using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.Repositories.Interfaces;

namespace RealEstateManagementSystem.Repositories.Implementations
{
    /// <summary>
    /// EF Core implementation of <see cref="IPropertyRepository"/>.
    /// Read queries use AsNoTracking because the results are only rendered,
    /// while the update flow loads tracked entities so changes can be persisted.
    /// </summary>
    public class PropertyRepository : IPropertyRepository
    {
        private readonly ApplicationDbContext _context;

        public PropertyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Reusable projection of the related data that screens need.</summary>
        private IQueryable<Property> PropertiesWithRelatedData()
        {
            return _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.PropertyType)
                .Include(p => p.Location)
                .Include(p => p.Images);
        }

        public async Task<IEnumerable<Property>> GetApprovedAsync()
        {
            return await PropertiesWithRelatedData()
                .AsNoTracking()
                .Where(p => p.Status == PropertyStatus.Approved)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetAllAsync(PropertyStatus? status = null)
        {
            var query = PropertiesWithRelatedData().AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetByOwnerIdAsync(string ownerId)
        {
            return await PropertiesWithRelatedData()
                .AsNoTracking()
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Property?> GetByIdAsync(int propertyId)
        {
            return await PropertiesWithRelatedData()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
        }

        public async Task<Property?> GetByIdForUpdateAsync(int propertyId)
        {
            // Tracked on purpose - the caller mutates the entity and calls SaveChangesAsync.
            return await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
        }

        public async Task<bool> ExistsAsync(int propertyId)
        {
            return await _context.Properties.AnyAsync(p => p.PropertyId == propertyId);
        }

        public async Task<int> CountByStatusAsync(PropertyStatus status)
        {
            return await _context.Properties.CountAsync(p => p.Status == status);
        }

        public async Task<int> CountByOwnerAsync(string ownerId)
        {
            return await _context.Properties.CountAsync(p => p.OwnerId == ownerId);
        }

        public async Task AddAsync(Property property)
        {
            await _context.Properties.AddAsync(property);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Property property)
        {
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Property property)
        {
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteImageAsync(PropertyImage image)
        {
            _context.PropertyImages.Remove(image);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
