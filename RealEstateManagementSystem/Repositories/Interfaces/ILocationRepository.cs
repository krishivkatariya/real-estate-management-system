using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.Repositories.Interfaces
{
    /// <summary>
    /// Repository abstraction for the Location lookup table.
    /// Locations are re-used by many properties and are never hard-coded in the property forms.
    /// </summary>
    public interface ILocationRepository
    {
        /// <summary>Every location (Admin management screen).</summary>
        Task<IEnumerable<Location>> GetAllAsync();

        /// <summary>Active locations only - used to populate the property form dropdown.</summary>
        Task<IEnumerable<Location>> GetActiveAsync();

        Task<Location?> GetByIdAsync(int locationId);

        /// <summary>Duplicate check on City + State + Pincode, ignoring the row currently being edited.</summary>
        Task<bool> ExistsAsync(string city, string state, string pincode, int? excludeLocationId = null);

        /// <summary>True when at least one property already uses this location.</summary>
        Task<bool> HasPropertiesAsync(int locationId);

        Task<int> CountPropertiesAsync(int locationId);

        Task AddAsync(Location location);

        Task UpdateAsync(Location location);

        Task SaveChangesAsync();
    }
}
