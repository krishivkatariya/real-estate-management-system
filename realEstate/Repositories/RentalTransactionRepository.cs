using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Repositories;

public class RentalTransactionRepository : IRentalTransactionRepository
{
    private readonly ApplicationDbContext _db;

    public RentalTransactionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    private IQueryable<RentalTransaction> WithRelatedData()
    {
        return _db.RentalTransactions
            .Include(rental => rental.Property)
                .ThenInclude(property => property!.Images)
            .Include(rental => rental.Renter)
            .Include(rental => rental.Owner);
    }

    public Task<RentalTransaction?> GetByIdAsync(int rentalTransactionId, CancellationToken cancellationToken = default)
    {
        return WithRelatedData().FirstOrDefaultAsync(rental => rental.RentalTransactionId == rentalTransactionId, cancellationToken);
    }

    public Task<List<RentalTransaction>> GetByRenterIdAsync(string renterId, CancellationToken cancellationToken = default)
    {
        return WithRelatedData().AsNoTracking()
            .Where(rental => rental.RenterId == renterId)
            .OrderByDescending(rental => rental.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<RentalTransaction>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return WithRelatedData().AsNoTracking()
            .Where(rental => rental.OwnerId == ownerId)
            .OrderByDescending(rental => rental.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<RentalTransaction>> GetByPropertyIdAsync(int propertyId, CancellationToken cancellationToken = default)
    {
        return WithRelatedData().AsNoTracking()
            .Where(rental => rental.PropertyId == propertyId)
            .OrderByDescending(rental => rental.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<RentalTransaction>> GetPendingForOwnerAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return WithRelatedData().AsNoTracking()
            .Where(rental => rental.OwnerId == ownerId && rental.Status == RentalStatus.Pending)
            .OrderBy(rental => rental.StartDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<RentalTransaction>> GetActiveRentalsAsync(CancellationToken cancellationToken = default)
    {
        return WithRelatedData().AsNoTracking()
            .Where(rental => rental.Status == RentalStatus.Active)
            .OrderBy(rental => rental.EndDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<RentalTransaction>> GetHistoryForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return WithRelatedData().AsNoTracking()
            .Where(rental => (rental.RenterId == userId || rental.OwnerId == userId)
                && (rental.Status == RentalStatus.Completed || rental.Status == RentalStatus.Rejected || rental.Status == RentalStatus.Cancelled))
            .OrderByDescending(rental => rental.EndDate)
            .ToListAsync(cancellationToken);
    }

    public Task<List<RentalTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return WithRelatedData().AsNoTracking()
            .OrderByDescending(rental => rental.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(int rentalTransactionId, CancellationToken cancellationToken = default)
    {
        return _db.RentalTransactions.AnyAsync(rental => rental.RentalTransactionId == rentalTransactionId, cancellationToken);
    }

    public Task<bool> HasOpenRequestAsync(string renterId, int propertyId, CancellationToken cancellationToken = default)
    {
        return _db.RentalTransactions.AnyAsync(rental =>
            rental.RenterId == renterId && rental.PropertyId == propertyId
            && (rental.Status == RentalStatus.Pending || rental.Status == RentalStatus.Approved || rental.Status == RentalStatus.Active),
            cancellationToken);
    }

    public Task<bool> HasOverlappingOpenRentalAsync(int propertyId, DateTime startDate, DateTime endDate, int? excludeRentalTransactionId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.RentalTransactions.Where(rental =>
            rental.PropertyId == propertyId
            && (rental.Status == RentalStatus.Pending
                || rental.Status == RentalStatus.Approved
                || rental.Status == RentalStatus.Active)
            && rental.StartDate < endDate
            && rental.EndDate > startDate);

        if (excludeRentalTransactionId.HasValue)
        {
            query = query.Where(rental => rental.RentalTransactionId != excludeRentalTransactionId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(RentalTransaction rentalTransaction, CancellationToken cancellationToken = default)
    {
        await _db.RentalTransactions.AddAsync(rentalTransaction, cancellationToken);
    }

    public Task UpdateAsync(RentalTransaction rentalTransaction, CancellationToken cancellationToken = default)
    {
        _db.RentalTransactions.Update(rentalTransaction);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}