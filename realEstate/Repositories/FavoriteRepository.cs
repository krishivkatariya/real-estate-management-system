using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly ApplicationDbContext _db;

    public FavoriteRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddFavoriteAsync(string userId, int propertyId, CancellationToken cancellationToken = default)
    {
        // prevent duplicates
        if (await IsFavoritedAsync(userId, propertyId, cancellationToken)) return;

        var fav = new Favorite { UserId = userId, PropertyId = propertyId };
        _db.Favorites.Add(fav);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Property>> GetFavoritesForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _db.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Property)
                .ThenInclude(p => p.Images)
            .Select(f => f.Property!)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> GetFavoritePropertyIdsForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _db.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.PropertyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsFavoritedAsync(string userId, int propertyId, CancellationToken cancellationToken = default)
    {
        return await _db.Favorites.AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId, cancellationToken);
    }

    public async Task RemoveFavoriteAsync(string userId, int propertyId, CancellationToken cancellationToken = default)
    {
        var fav = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId, cancellationToken);
        if (fav == null) return;
        _db.Favorites.Remove(fav);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
