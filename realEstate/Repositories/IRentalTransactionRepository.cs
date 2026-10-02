using realEstate.Models;

namespace realEstate.Repositories;

public interface IRentalTransactionRepository
{
    Task<RentalTransaction?> GetByIdAsync(int rentalTransactionId, CancellationToken cancellationToken = default);
    Task<List<RentalTransaction>> GetByRenterIdAsync(string renterId, CancellationToken cancellationToken = default);
    Task<List<RentalTransaction>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
    Task<List<RentalTransaction>> GetByPropertyIdAsync(int propertyId, CancellationToken cancellationToken = default);
    Task<List<RentalTransaction>> GetPendingForOwnerAsync(string ownerId, CancellationToken cancellationToken = default);
    Task<List<RentalTransaction>> GetActiveRentalsAsync(CancellationToken cancellationToken = default);
    Task<List<RentalTransaction>> GetHistoryForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<RentalTransaction>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int rentalTransactionId, CancellationToken cancellationToken = default);
    Task<bool> HasOpenRequestAsync(string renterId, int propertyId, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingOpenRentalAsync(int propertyId, DateTime startDate, DateTime endDate, int? excludeRentalTransactionId = null, CancellationToken cancellationToken = default);
    Task AddAsync(RentalTransaction rentalTransaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(RentalTransaction rentalTransaction, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}