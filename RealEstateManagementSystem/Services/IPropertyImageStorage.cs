using Microsoft.AspNetCore.Http;

namespace RealEstateManagementSystem.Services
{
    /// <summary>
    /// Handles the physical storage of property images under wwwroot/uploads/properties.
    /// Image binaries are never stored inside SQL Server - only the resulting URL is saved
    /// in the PropertyImage table.
    /// </summary>
    public interface IPropertyImageStorage
    {
        /// <summary>Maximum number of images that may be attached to a single property.</summary>
        int MaxImagesPerProperty { get; }

        /// <summary>Maximum allowed size of a single uploaded image (bytes).</summary>
        long MaxFileSizeInBytes { get; }

        /// <summary>Allowed file extensions, lower case and including the dot.</summary>
        IReadOnlyList<string> AllowedExtensions { get; }

        /// <summary>
        /// Validates extension, size and content type of an uploaded file.
        /// Returns an error message, or null when the file is acceptable.
        /// </summary>
        string? Validate(IFormFile file);

        /// <summary>
        /// Stores the file with a unique generated name and returns its relative web URL,
        /// e.g. "/uploads/properties/3f2b....jpg".
        /// </summary>
        Task<string> SaveAsync(IFormFile file);

        /// <summary>
        /// Removes a previously stored file. Silently ignores null/unknown paths and
        /// refuses to touch anything outside the upload folder.
        /// </summary>
        void Delete(string? imageUrl);
    }
}
