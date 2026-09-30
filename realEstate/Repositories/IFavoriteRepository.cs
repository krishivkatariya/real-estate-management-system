using realEstate.Models;

namespace realEstate.Repositories;

public interface IFavoriteRepository
{
    Task<bool> IsFavoritedAsync(string userId, int propertyId, CancellationToken cancellationToken = default);
    Task AddFavoriteAsync(string userId, int propertyId, CancellationToken cancellationToken = default);
    Task RemoveFavoriteAsync(string userId, int propertyId, CancellationToken cancellationToken = default);
    Task<List<int>> GetFavoritePropertyIdsForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Property>> GetFavoritesForUserAsync(string userId, CancellationToken cancellationToken = default);
}
