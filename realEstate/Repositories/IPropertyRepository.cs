using realEstate.Models;

namespace realEstate.Repositories;

public interface IPropertyRepository
{
    Task<IEnumerable<Property>> GetAllAsync(CancellationToken cancellationToken = default);
    IQueryable<Property> Query();
    Task<Property?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Property property, CancellationToken cancellationToken = default);
    Task UpdateAsync(Property property, CancellationToken cancellationToken = default);
    Task DeleteAsync(Property property, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
