using Microsoft.AspNetCore.Http;
using realEstate.Models;

namespace realEstate.Services;

public interface IImageService
{
    Task<PropertyImage> SavePropertyImageAsync(IFormFile file, int propertyId, CancellationToken cancellationToken = default);
    Task DeletePropertyImageAsync(PropertyImage image, CancellationToken cancellationToken = default);
}
