using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _db;

    public PropertyRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Property property, CancellationToken cancellationToken = default)
    {
        await _db.Properties.AddAsync(property, cancellationToken);
    }

    public async Task DeleteAsync(Property property, CancellationToken cancellationToken = default)
    {
        _db.Properties.Remove(property);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Property>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Properties
            .Include(p => p.Images)
            .Include(p => p.Agent)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public IQueryable<Property> Query()
    {
        return _db.Properties
            .Include(p => p.Images)
            .Include(p => p.Agent)
            .Include(p => p.Owner)
            .AsNoTracking();
    }

    public async Task<Property?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Properties
            .Include(p => p.Images)
            .Include(p => p.Agent)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Property property, CancellationToken cancellationToken = default)
    {
        _db.Properties.Update(property);
        await Task.CompletedTask;
    }
}
