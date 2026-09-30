using realEstate.Models;

namespace realEstate.Repositories;

public interface IPurchaseRequestRepository
{
    Task<PurchaseRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<PurchaseRequest>> GetRequestsByBuyerAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<PurchaseRequest>> GetRequestsForOwnerAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task AddAsync(PurchaseRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(PurchaseRequest request, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsOpenRequestAsync(string userId, int propertyId, CancellationToken cancellationToken = default);
}
