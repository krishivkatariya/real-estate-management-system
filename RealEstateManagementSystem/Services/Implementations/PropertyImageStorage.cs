using Microsoft.AspNetCore.Http;

namespace RealEstateManagementSystem.Services.Implementations
{
    /// <summary>
    /// Stores uploaded property images on disk under wwwroot/uploads/properties.
    /// The original file name is never reused: a new GUID based file name is generated so that
    /// two users can upload "photo.jpg" without overwriting each other, and so the folder
    /// cannot be probed with a predictable path.
    /// </summary>
    public class PropertyImageStorage : IPropertyImageStorage
    {
        /// <summary>Folder below wwwroot where property images are kept.</summary>
        public const string UploadFolderName = "properties";

        /// <summary>Relative URL prefix that is stored in PropertyImage.ImageUrl.</summary>
        public const string UploadUrlPrefix = "/uploads/" + UploadFolderName;

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private const int MaxImages = 8;

        private static readonly string[] Extensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PropertyImageStorage> _logger;

        public PropertyImageStorage(IWebHostEnvironment environment, ILogger<PropertyImageStorage> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public int MaxImagesPerProperty => MaxImages;

        public long MaxFileSizeInBytes => MaxFileSize;

        public IReadOnlyList<string> AllowedExtensions => Extensions;

        public string? Validate(IFormFile file)
        {
            if (file == null)
            {
                return "The uploaded file could not be read.";
            }

            if (file.Length <= 0)
            {
                return $"'{file.FileName}' is empty.";
            }

            if (file.Length > MaxFileSize)
            {
                return $"'{file.FileName}' is larger than the maximum allowed size of {MaxFileSize / (1024 * 1024)} MB.";
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Extensions.Contains(extension))
            {
                return $"'{file.FileName}' is not an allowed image format. Allowed formats: {string.Join(", ", Extensions)}.";
            }

            if (!string.IsNullOrWhiteSpace(file.ContentType) &&
                !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return $"'{file.FileName}' does not look like an image file.";
            }

            return null;
        }

        public async Task<string> SaveAsync(IFormFile file)
        {
            var folder = EnsureUploadFolder();

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var generatedName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(folder, generatedName);

            await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Stored property image {FileName} ({Length} bytes).", generatedName, file.Length);

            return $"{UploadUrlPrefix}/{generatedName}";
        }

        public void Delete(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            try
            {
                var folder = EnsureUploadFolder();
                var fileName = Path.GetFileName(imageUrl.Trim());
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return;
                }

                var fullPath = Path.Combine(folder, fileName);

                // Safety net: the resolved path must stay inside the upload folder.
                if (!Path.GetFullPath(fullPath).StartsWith(Path.GetFullPath(folder), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Refused to delete image outside the upload folder: {ImageUrl}", imageUrl);
                    return;
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // A missing file must never break the workflow.
                _logger.LogWarning(ex, "Could not delete property image {ImageUrl}.", imageUrl);
            }
        }

        /// <summary>Returns the physical upload folder, creating it when needed.</summary>
        private string EnsureUploadFolder()
        {
            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            var folder = Path.Combine(webRoot, "uploads", UploadFolderName);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}
