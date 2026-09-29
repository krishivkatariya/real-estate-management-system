using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.Repositories.Interfaces
{
    /// <summary>
    /// Repository abstraction for the PropertyType lookup table.
    /// Types are never deleted - they are deactivated with IsActive so existing listings stay valid.
    /// </summary>
    public interface IPropertyTypeRepository
    {
        /// <summary>Every property type (Admin management screen).</summary>
        Task<IEnumerable<PropertyType>> GetAllAsync();

        /// <summary>Active property types only - used to populate the property form dropdown.</summary>
        Task<IEnumerable<PropertyType>> GetActiveAsync();

        Task<PropertyType?> GetByIdAsync(int propertyTypeId);

        /// <summary>Name uniqueness check; <paramref name="excludePropertyTypeId"/> ignores the row being edited.</summary>
        Task<bool> ExistsByNameAsync(string name, int? excludePropertyTypeId = null);

        /// <summary>True when at least one property already uses this type, therefore it cannot be deleted.</summary>
        Task<bool> HasPropertiesAsync(int propertyTypeId);

        /// <summary>How many properties use this type - shown as a warning in the Admin screen.</summary>
        Task<int> CountPropertiesAsync(int propertyTypeId);

        Task AddAsync(PropertyType propertyType);

        Task UpdateAsync(PropertyType propertyType);

        Task SaveChangesAsync();
    }
}
