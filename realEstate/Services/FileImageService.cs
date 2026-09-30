using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Services;

public class FileImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _db;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB
    private readonly string[] _permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

    public FileImageService(IWebHostEnvironment env, ApplicationDbContext db)
    {
        _env = env;
        _db = db;
    }

    public async Task<PropertyImage> SavePropertyImageAsync(IFormFile file, int propertyId, CancellationToken cancellationToken = default)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        if (file.Length == 0) throw new ArgumentException("Empty file", nameof(file));
        if (file.Length > _maxFileSize) throw new ArgumentException("File too large", nameof(file));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            throw new ArgumentException("Invalid file type", nameof(file));

        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "properties", propertyId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var fileName = Guid.NewGuid().ToString("N") + ext;
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = Path.Combine("/uploads/properties", propertyId.ToString(), fileName).Replace("\\", "/");

        var image = new PropertyImage
        {
            PropertyId = propertyId,
            FileName = fileName,
            FilePath = relativePath,
            IsPrimary = false
        };

        _db.PropertyImages.Add(image);
        await _db.SaveChangesAsync(cancellationToken);

        return image;
    }

    public async Task DeletePropertyImageAsync(PropertyImage image, CancellationToken cancellationToken = default)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));

        var wwwRoot = _env.WebRootPath;
        var fullPath = Path.Combine(wwwRoot, image.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath)) File.Delete(fullPath);

        _db.PropertyImages.Remove(image);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
