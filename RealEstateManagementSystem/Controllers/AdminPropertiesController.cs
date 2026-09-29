using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.Models.ViewModels;
using RealEstateManagementSystem.Repositories.Interfaces;
using RealEstateManagementSystem.Services;

namespace RealEstateManagementSystem.Controllers
{
    /// <summary>
    /// Phase 2 - Admin moderation of property listings (approve / reject / remove).
    /// Every action is protected by [Authorize(Roles = "Admin")] so a normal user can never
    /// reach these URLs even by typing them manually.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("Admin/Properties")]
    public class AdminPropertiesController : Controller
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageStorage _imageStorage;
        private readonly ILogger<AdminPropertiesController> _logger;

        public AdminPropertiesController(
            IPropertyRepository propertyRepository,
            IPropertyImageStorage imageStorage,
            ILogger<AdminPropertiesController> logger)
        {
            _propertyRepository = propertyRepository;
            _imageStorage = imageStorage;
            _logger = logger;
        }

        // GET: /Admin/Properties
        // GET: /Admin/Properties?status=1
        [HttpGet("")]
        public async Task<IActionResult> Index(PropertyStatus? status)
        {
            ViewData["Title"] = "Manage Properties";

            if (status.HasValue && !Enum.IsDefined(typeof(PropertyStatus), status.Value))
            {
                status = null;
            }

            ViewData["StatusFilter"] = status;
            ViewData["TotalCount"] = (await _propertyRepository.GetAllAsync()).Count();
            ViewData["PendingCount"] = await _propertyRepository.CountByStatusAsync(PropertyStatus.Pending);
            ViewData["ApprovedCount"] = await _propertyRepository.CountByStatusAsync(PropertyStatus.Approved);
            ViewData["RejectedCount"] = await _propertyRepository.CountByStatusAsync(PropertyStatus.Rejected);

            var properties = await _propertyRepository.GetAllAsync(status);
            return View(properties);
        }

        // POST: /Admin/Properties/Approve/5
        [HttpPost("Approve/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, PropertyStatus? status)
        {
            var property = await _propertyRepository.GetByIdForUpdateAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            property.Status = PropertyStatus.Approved;
            property.RejectionReason = null;
            property.UpdatedAt = DateTime.UtcNow;

            await _propertyRepository.UpdateAsync(property);

            _logger.LogInformation("Admin {Admin} approved property {PropertyId}.", User.Identity?.Name, id);
            TempData["Success"] = $"'{property.Title}' was approved and is now visible to everyone.";
            return RedirectToAction(nameof(Index), new { status });
        }

        // GET: /Admin/Properties/Reject/5
        [HttpGet("Reject/{id:int}")]
        public async Task<IActionResult> Reject(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Reject Property";
            var model = new PropertyRejectViewModel
            {
                PropertyId = property.PropertyId,
                Title = property.Title,
                RejectionReason = property.RejectionReason ?? string.Empty
            };

            return View(model);
        }

        // POST: /Admin/Properties/Reject/5
        [HttpPost("Reject/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, PropertyRejectViewModel model)
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

            if (!ModelState.IsValid)
            {
                model.Title = property.Title;
                ViewData["Title"] = "Reject Property";
                return View(model);
            }

            property.Status = PropertyStatus.Rejected;
            property.RejectionReason = model.RejectionReason.Trim();
            property.UpdatedAt = DateTime.UtcNow;

            await _propertyRepository.UpdateAsync(property);

            _logger.LogInformation("Admin {Admin} rejected property {PropertyId}.", User.Identity?.Name, id);
            TempData["Success"] = $"'{property.Title}' was rejected. The owner can now see the reason and edit the listing.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Properties/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Remove Property";
            return View(property);
        }

        // POST: /Admin/Properties/Delete/5
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _propertyRepository.GetByIdForUpdateAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            var title = property.Title;
            var imageUrls = property.Images.Select(image => image.ImageUrl).ToList();

            try
            {
                await _propertyRepository.DeleteAsync(property);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin failed to delete property {PropertyId}.", id);
                TempData["Error"] = "This property could not be removed because it is linked to other records.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var url in imageUrls)
            {
                _imageStorage.Delete(url);
            }

            _logger.LogInformation("Admin {Admin} deleted property {PropertyId}.", User.Identity?.Name, id);
            TempData["Success"] = $"'{title}' was removed from the system.";
            return RedirectToAction(nameof(Index));
        }
    }
}
