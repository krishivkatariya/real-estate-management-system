using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Repositories;

public class PurchaseRequestRepository : IPurchaseRequestRepository
{
    private readonly ApplicationDbContext _db;

    public PurchaseRequestRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        await _db.PurchaseRequests.AddAsync(request, cancellationToken);
    }

    public async Task<PurchaseRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.PurchaseRequests
            .Include(r => r.Property)
                .ThenInclude(p => p.Images)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<PurchaseRequest>> GetRequestsByBuyerAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _db.PurchaseRequests
            .Where(r => r.UserId == userId)
            .Include(r => r.Property)
                .ThenInclude(p => p.Images)
            .AsNoTracking()
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PurchaseRequest>> GetRequestsForOwnerAsync(string ownerUserId, CancellationToken cancellationToken = default)
    {
        return await _db.PurchaseRequests
            .Include(r => r.Property)
                .ThenInclude(p => p.Images)
            .Include(r => r.User)
            .Where(r => r.Property != null && r.Property.OwnerId == ownerUserId)
            .AsNoTracking()
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsOpenRequestAsync(string userId, int propertyId, CancellationToken cancellationToken = default)
    {
        return await _db.PurchaseRequests.AnyAsync(r => r.UserId == userId && r.PropertyId == propertyId && r.Status == PurchaseRequestStatus.Pending, cancellationToken);
    }

    public async Task UpdateAsync(PurchaseRequest request, CancellationToken cancellationToken = default)
    {
        _db.PurchaseRequests.Update(request);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}
