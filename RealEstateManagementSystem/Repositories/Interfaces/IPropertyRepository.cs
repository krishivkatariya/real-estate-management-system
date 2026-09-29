using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.Repositories.Interfaces
{
    /// <summary>
    /// Repository abstraction for property listings.
    /// All Entity Framework Core / LINQ work for properties lives here so that the
    /// controllers only deal with orchestration, validation and authorization.
    /// </summary>
    public interface IPropertyRepository
    {
        /// <summary>Approved listings for the public property page, newest first.</summary>
        Task<IEnumerable<Property>> GetApprovedAsync();

        /// <summary>Every listing, optionally restricted to a single status (Admin area).</summary>
        Task<IEnumerable<Property>> GetAllAsync(PropertyStatus? status = null);

        /// <summary>Listings that belong to one owner (the logged-in user's own listings).</summary>
        Task<IEnumerable<Property>> GetByOwnerIdAsync(string ownerId);

        /// <summary>Single listing with Owner, PropertyType, Location and Images loaded.</summary>
        Task<Property?> GetByIdAsync(int propertyId);

        /// <summary>
        /// Single tracked listing with its images, used by the update flow so EF Core can
        /// detect the changes. Returns null when the listing does not exist.
        /// </summary>
        Task<Property?> GetByIdForUpdateAsync(int propertyId);

        Task<bool> ExistsAsync(int propertyId);

        /// <summary>Number of listings in a status - used for the admin summary badges.</summary>
        Task<int> CountByStatusAsync(PropertyStatus status);

        /// <summary>Number of listings owned by one user.</summary>
        Task<int> CountByOwnerAsync(string ownerId);

        Task AddAsync(Property property);

        Task UpdateAsync(Property property);

        /// <summary>Deletes a listing; its image rows are removed by cascade delete.</summary>
        Task DeleteAsync(Property property);

        /// <summary>Removes a single image row (the physical file is handled by the file storage service).</summary>
        Task DeleteImageAsync(PropertyImage image);

        Task SaveChangesAsync();
    }
}
