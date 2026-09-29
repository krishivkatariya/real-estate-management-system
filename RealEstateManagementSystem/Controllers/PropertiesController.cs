using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.Models.ViewModels;
using RealEstateManagementSystem.Repositories.Interfaces;
using RealEstateManagementSystem.Services;

namespace RealEstateManagementSystem.Controllers
{
    /// <summary>
    /// Phase 2 - Property Management for normal authenticated users.
    /// The same account can act as an owner/seller AND as a buyer/renter, so no separate
    /// Seller or Buyer account exists anywhere in the system.
    ///
    /// Auth rules enforced here (not only hidden in the UI):
    ///  - OwnerId always comes from the logged-in user, never from the request.
    ///  - Only the owner or an Administrator may edit / delete a listing.
    ///  - Only Approved listings are listed publicly.
    /// </summary>
    [Authorize]
    public class PropertiesController : Controller
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IPropertyImageStorage _imageStorage;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PropertiesController> _logger;

        public PropertiesController(
            IPropertyRepository propertyRepository,
            IPropertyTypeRepository propertyTypeRepository,
            ILocationRepository locationRepository,
            IPropertyImageStorage imageStorage,
            UserManager<ApplicationUser> userManager,
            ILogger<PropertiesController> logger)
        {
            _propertyRepository = propertyRepository;
            _propertyTypeRepository = propertyTypeRepository;
            _locationRepository = locationRepository;
            _imageStorage = imageStorage;
            _userManager = userManager;
            _logger = logger;
        }

        // ------------------------------------------------------------------
        // Public listing (approved listings only)
        // ------------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Properties";
            var properties = await _propertyRepository.GetApprovedAsync();
            return View(properties);
        }

        // ------------------------------------------------------------------
        // The current user's own listings
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> MyProperties()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            ViewData["Title"] = "My Properties";
            var properties = await _propertyRepository.GetByOwnerIdAsync(userId);
            return View(properties);
        }

        // ------------------------------------------------------------------
        // Details
        // ------------------------------------------------------------------
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            var canManage = CanManage(property);
            var isRestricted = property.Status != PropertyStatus.Approved;

            // Pending / Rejected / Inactive listings must not be discoverable by other users.
            if (isRestricted && !canManage)
            {
                return NotFound();
            }

            var model = new PropertyDetailsViewModel
            {
                Property = property,
                OwnerName = property.Owner?.FullName ?? "Property Owner",
                CanManage = canManage,
                IsRestrictedListing = isRestricted
            };

            ViewData["Title"] = property.Title;
            return View(model);
        }

        // ------------------------------------------------------------------
        // Create
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Add Property";
            var model = new PropertyCreateViewModel();
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyCreateViewModel model)
        {
            await ValidateLookupSelectionsAsync(model);
            var files = GetUploadedFiles(model.Images);
            ValidateUploadedFiles(files);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var savedUrls = new List<string>();
            try
            {
                foreach (var file in files)
                {
                    savedUrls.Add(await _imageStorage.SaveAsync(file));
                }

                var primaryUrl = savedUrls.FirstOrDefault();

                // OwnerId / Status / CreatedAt are decided here, never trusted from the form.
                var property = new Property
                {
                    Title = model.Title.Trim(),
                    Description = model.Description.Trim(),
                    Price = model.Price!.Value,
                    ListingType = model.ListingType!.Value,
                    PropertyTypeId = model.PropertyTypeId!.Value,
                    LocationId = model.LocationId!.Value,
                    Address = model.Address.Trim(),
                    Area = model.Area!.Value,
                    Bedrooms = model.Bedrooms,
                    Bathrooms = model.Bathrooms,
                    OwnerId = _userManager.GetUserId(User)!,
                    Status = PropertyStatus.Pending,
                    RejectionReason = null,
                    CreatedAt = DateTime.UtcNow,
                    Images = savedUrls
                        .Select(url => new PropertyImage
                        {
                            ImageUrl = url,
                            IsPrimary = string.Equals(url, primaryUrl, StringComparison.Ordinal)
                        })
                        .ToList()
                };

                await _propertyRepository.AddAsync(property);
            }
            catch (Exception ex)
            {
                // Never leave orphaned files behind if the database write fails.
                foreach (var url in savedUrls)
                {
                    _imageStorage.Delete(url);
                }

                _logger.LogError(ex, "Failed to create property listing for user {UserId}.", _userManager.GetUserId(User));
                ModelState.AddModelError(string.Empty, "The listing could not be saved. Please try again.");
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            TempData["Success"] = "Your property was submitted successfully and is waiting for admin approval.";
            return RedirectToAction(nameof(MyProperties));
        }

        // ------------------------------------------------------------------
        // Edit (owner or admin only)
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            // Non-owners get a 404 so listing ids cannot be probed.
            if (!CanManage(property))
            {
                return NotFound();
            }

            ViewData["Title"] = "Edit Property";
            var model = BuildEditViewModel(property);
            await PopulateDropdownsAsync(model, property.PropertyTypeId, property.LocationId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PropertyEditViewModel model)
        {
            if (id != model.PropertyId)
            {
                return BadRequest();
            }

            var property = await _propertyRepository.GetByIdForUpdateAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            if (!CanManage(property))
            {
                return NotFound();
            }

            var files = GetUploadedFiles(model.Images);
            await ValidateEditLookupSelectionsAsync(model, property);
            ValidateEditImages(model, property, files);

            if (!ModelState.IsValid)
            {
                return await RedisplayEditAsync(model, property);
            }

            // Anything the owner changes means the content has to be reviewed again.
            bool contentChanged =
                !string.Equals(property.Title, model.Title.Trim(), StringComparison.Ordinal) ||
                !string.Equals(property.Description, model.Description.Trim(), StringComparison.Ordinal) ||
                !string.Equals(property.Address, model.Address.Trim(), StringComparison.Ordinal) ||
                property.Price != model.Price!.Value ||
                property.Area != model.Area!.Value ||
                property.Bedrooms != model.Bedrooms ||
                property.Bathrooms != model.Bathrooms ||
                property.ListingType != model.ListingType!.Value ||
                property.PropertyTypeId != model.PropertyTypeId!.Value ||
                property.LocationId != model.LocationId!.Value;

            var imagesToRemove = property.Images
                .Where(image => model.RemoveImageIds.Contains(image.PropertyImageId))
                .ToList();

            var savedUrls = new List<string>();
            try
            {
                if (contentChanged || imagesToRemove.Count > 0 || files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        savedUrls.Add(await _imageStorage.SaveAsync(file));
                    }
                }

                foreach (var image in imagesToRemove)
                {
                    property.Images.Remove(image);
                    await _propertyRepository.DeleteImageAsync(image);
                }

                foreach (var url in savedUrls)
                {
                    property.Images.Add(new PropertyImage { ImageUrl = url, IsPrimary = false });
                }

                ApplyPrimaryImageSelection(property, model.PrimaryImageId);

                property.Title = model.Title.Trim();
                property.Description = model.Description.Trim();
                property.Address = model.Address.Trim();
                property.Price = model.Price!.Value;
                property.Area = model.Area!.Value;
                property.Bedrooms = model.Bedrooms;
                property.Bathrooms = model.Bathrooms;
                property.ListingType = model.ListingType!.Value;
                property.PropertyTypeId = model.PropertyTypeId!.Value;
                property.LocationId = model.LocationId!.Value;
                property.UpdatedAt = DateTime.UtcNow;

                if (property.Status == PropertyStatus.Rejected || property.Status == PropertyStatus.Approved)
                {
                    property.Status = PropertyStatus.Pending;
                    property.RejectionReason = null;
                }

                await _propertyRepository.UpdateAsync(property);
            }
            catch (Exception ex)
            {
                foreach (var url in savedUrls)
                {
                    _imageStorage.Delete(url);
                }

                _logger.LogError(ex, "Failed to update property {PropertyId}.", id);
                ModelState.AddModelError(string.Empty, "The changes could not be saved. Please try again.");
                return await RedisplayEditAsync(model, property);
            }

            // Physical files are removed only after the database delete succeeded.
            foreach (var image in imagesToRemove)
            {
                _imageStorage.Delete(image.ImageUrl);
            }

            TempData["Success"] = "Property updated successfully. Edited listings are sent back for approval.";
            return RedirectToAction(nameof(MyProperties));
        }

        // ------------------------------------------------------------------
        // Delete (owner or admin only)
        // ------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            if (!CanManage(property))
            {
                return NotFound();
            }

            ViewData["Title"] = "Delete Property";
            return View(property);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _propertyRepository.GetByIdForUpdateAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            if (!CanManage(property))
            {
                return NotFound();
            }

            // Keep the file names so the physical files can be cleaned up after the row is gone.
            var imageUrls = property.Images.Select(image => image.ImageUrl).ToList();

            try
            {
                await _propertyRepository.DeleteAsync(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete property {PropertyId}.", id);

                // A listing that is referenced by other records cannot be removed safely.
                TempData["Error"] = "This property could not be deleted because it is linked to other records.";
                return RedirectToAction(nameof(MyProperties));
            }

            foreach (var url in imageUrls)
            {
                _imageStorage.Delete(url);
            }

            TempData["Success"] = "Property deleted successfully.";
            return RedirectToAction(nameof(MyProperties));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>True when the current visitor owns the listing or is an Administrator.</summary>
        private bool CanManage(Property property)
        {
            if (property == null)
            {
                return false;
            }

            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var userId = _userManager.GetUserId(User);
            return !string.IsNullOrEmpty(userId) &&
                   string.Equals(property.OwnerId, userId, StringComparison.Ordinal);
        }

        /// <summary>Only files the user actually selected are processed.</summary>
        private static List<IFormFile> GetUploadedFiles(List<IFormFile>? files)
        {
            return files?.Where(file => file is { Length: > 0 }).ToList() ?? new List<IFormFile>();
        }

        /// <summary>Extension / size / content-type checks for every new upload.</summary>
        private void ValidateUploadedFiles(List<IFormFile> files)
        {
            if (files.Count == 0)
            {
                return;
            }

            if (files.Count > _imageStorage.MaxImagesPerProperty)
            {
                ModelState.AddModelError(
                    nameof(PropertyCreateViewModel.Images),
                    $"You can upload a maximum of {_imageStorage.MaxImagesPerProperty} images per property.");
                return;
            }

            foreach (var file in files)
            {
                var error = _imageStorage.Validate(file);
                if (error != null)
                {
                    ModelState.AddModelError(nameof(PropertyCreateViewModel.Images), error);
                }
            }
        }

        /// <summary>Rejects property type / location ids that do not exist or are switched off.</summary>
        private async Task ValidateLookupSelectionsAsync(PropertyCreateViewModel model)
        {
            var propertyType = model.PropertyTypeId.HasValue
                ? await _propertyTypeRepository.GetByIdAsync(model.PropertyTypeId.Value)
                : null;

            if (propertyType == null || !propertyType.IsActive)
            {
                ModelState.AddModelError(nameof(model.PropertyTypeId), "Please select a valid property type.");
            }

            var location = model.LocationId.HasValue
                ? await _locationRepository.GetByIdAsync(model.LocationId.Value)
                : null;

            if (location == null || !location.IsActive)
            {
                ModelState.AddModelError(nameof(model.LocationId), "Please select a valid location.");
            }
        }

        /// <summary>Fills the Create form dropdowns with the active lookup rows.</summary>
        private async Task PopulateDropdownsAsync(PropertyCreateViewModel model)
        {
            var propertyTypes = await _propertyTypeRepository.GetActiveAsync();
            var locations = await _locationRepository.GetActiveAsync();

            model.PropertyTypes = propertyTypes
                .Select(type => new SelectListItem { Value = type.PropertyTypeId.ToString(), Text = type.Name })
                .ToList();

            model.Locations = locations
                .Select(location => new SelectListItem { Value = location.LocationId.ToString(), Text = location.DisplayName })
                .ToList();
        }

        /// <summary>Fills the Edit form dropdowns, keeping a currently used but deactivated row selectable.</summary>
        private async Task PopulateDropdownsAsync(PropertyEditViewModel model, int propertyTypeId, int locationId)
        {
            var propertyTypes = await _propertyTypeRepository.GetAllAsync();
            var locations = await _locationRepository.GetAllAsync();

            model.PropertyTypes = propertyTypes
                .Where(type => type.IsActive || type.PropertyTypeId == propertyTypeId)
                .Select(type => new SelectListItem
                {
                    Value = type.PropertyTypeId.ToString(),
                    Text = type.IsActive ? type.Name : $"{type.Name} (inactive)"
                })
                .ToList();

            model.Locations = locations
                .Where(location => location.IsActive || location.LocationId == locationId)
                .Select(location => new SelectListItem
                {
                    Value = location.LocationId.ToString(),
                    Text = location.IsActive ? location.DisplayName : $"{location.DisplayName} (inactive)"
                })
                .ToList();
        }

        /// <summary>Maps a stored listing onto the edit form.</summary>
        private static PropertyEditViewModel BuildEditViewModel(Property property)
        {
            return new PropertyEditViewModel
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                Description = property.Description,
                Price = property.Price,
                ListingType = property.ListingType,
                PropertyTypeId = property.PropertyTypeId,
                LocationId = property.LocationId,
                Address = property.Address,
                Area = property.Area,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                CurrentStatus = property.Status,
                RejectionReason = property.RejectionReason,
                OwnerName = property.Owner?.FullName ?? "Property Owner",
                CreatedAt = property.CreatedAt,
                ExistingImages = property.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.PropertyImageId)
                    .ToList()
            };
        }

        /// <summary>Re-renders the edit form with the dropdowns and current images in place.</summary>
        private async Task<IActionResult> RedisplayEditAsync(PropertyEditViewModel model, Property property)
        {
            var refreshed = BuildEditViewModel(property);

            model.CurrentStatus = refreshed.CurrentStatus;
            model.RejectionReason = refreshed.RejectionReason;
            model.OwnerName = refreshed.OwnerName;
            model.CreatedAt = refreshed.CreatedAt;
            model.ExistingImages = refreshed.ExistingImages;

            await PopulateDropdownsAsync(model, property.PropertyTypeId, property.LocationId);

            ViewData["Title"] = "Edit Property";
            return View(nameof(Edit), model);
        }

        /// <summary>Validates the edit form lookups; a deactivated row stays valid while it is unchanged.</summary>
        private async Task ValidateEditLookupSelectionsAsync(PropertyEditViewModel model, Property property)
        {
            var propertyType = model.PropertyTypeId.HasValue
                ? await _propertyTypeRepository.GetByIdAsync(model.PropertyTypeId.Value)
                : null;

            if (propertyType == null || (!propertyType.IsActive && propertyType.PropertyTypeId != property.PropertyTypeId))
            {
                ModelState.AddModelError(nameof(model.PropertyTypeId), "Please select a valid property type.");
            }

            var location = model.LocationId.HasValue
                ? await _locationRepository.GetByIdAsync(model.LocationId.Value)
                : null;

            if (location == null || (!location.IsActive && location.LocationId != property.LocationId))
            {
                ModelState.AddModelError(nameof(model.LocationId), "Please select a valid location.");
            }
        }

        /// <summary>Validates the images of the edit form: the maximum image count covers old + new images.</summary>
        private void ValidateEditImages(PropertyEditViewModel model, Property property, List<IFormFile> files)
        {
            var remaining = property.Images.Count(image => !model.RemoveImageIds.Contains(image.PropertyImageId));

            if (remaining + files.Count > _imageStorage.MaxImagesPerProperty)
            {
                ModelState.AddModelError(
                    nameof(PropertyEditViewModel.Images),
                    $"A property can have at most {_imageStorage.MaxImagesPerProperty} images. " +
                    $"This listing would end up with {remaining + files.Count}.");
            }

            foreach (var file in files)
            {
                var error = _imageStorage.Validate(file);
                if (error != null)
                {
                    ModelState.AddModelError(nameof(PropertyEditViewModel.Images), error);
                }
            }

            var removingIds = property.Images
                .Where(image => model.RemoveImageIds.Contains(image.PropertyImageId))
                .Select(image => image.PropertyImageId)
                .ToList();

            if (model.PrimaryImageId.HasValue && removingIds.Contains(model.PrimaryImageId.Value))
            {
                ModelState.AddModelError(
                    nameof(PropertyEditViewModel.PrimaryImageId),
                    "The image chosen as primary cannot be removed at the same time.");
            }

            if (remaining == 0 && files.Count == 0 && property.Images.Count > 0)
            {
                ModelState.AddModelError(
                    nameof(PropertyEditViewModel.Images),
                    "Keep at least one image or upload a replacement before saving.");
            }
        }

        /// <summary>Marks exactly one image as primary, falling back to the first available image.</summary>
        private static void ApplyPrimaryImageSelection(Property property, int? requestedPrimaryImageId)
        {
            if (property.Images.Count == 0)
            {
                return;
            }

            var primary = requestedPrimaryImageId.HasValue
                ? property.Images.FirstOrDefault(image => image.PropertyImageId == requestedPrimaryImageId.Value)
                : null;

            primary ??= property.Images.FirstOrDefault();

            foreach (var image in property.Images)
            {
                image.IsPrimary = ReferenceEquals(image, primary);
            }
        }
    }
}
