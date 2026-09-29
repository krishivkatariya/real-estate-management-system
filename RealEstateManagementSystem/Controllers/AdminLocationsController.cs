using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.Models.ViewModels;
using RealEstateManagementSystem.Repositories.Interfaces;

namespace RealEstateManagementSystem.Controllers
{
    /// <summary>
    /// Phase 2 - Admin management of the Location lookup table.
    /// Locations are re-used by many properties and are never hard-coded in the property form.
    /// Like property types they are deactivated instead of deleted so existing listings stay valid.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("Admin/Locations")]
    public class AdminLocationsController : Controller
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ILogger<AdminLocationsController> _logger;

        public AdminLocationsController(
            ILocationRepository locationRepository,
            ILogger<AdminLocationsController> logger)
        {
            _locationRepository = locationRepository;
            _logger = logger;
        }

        // GET: /Admin/Locations
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Locations";

            var locations = await _locationRepository.GetAllAsync();
            var items = new List<LocationListItemViewModel>();

            foreach (var location in locations)
            {
                items.Add(new LocationListItemViewModel
                {
                    Location = location,
                    PropertyCount = await _locationRepository.CountPropertiesAsync(location.LocationId)
                });
            }

            return View(items);
        }

        // GET: /Admin/Locations/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Location";
            return View(new LocationFormViewModel());
        }

        // POST: /Admin/Locations/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LocationFormViewModel model)
        {
            if (await _locationRepository.ExistsAsync(model.City.Trim(), model.State.Trim(), model.Pincode.Trim()))
            {
                ModelState.AddModelError(string.Empty, "This location (city, state and pincode) already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Add Location";
                return View(model);
            }

            await _locationRepository.AddAsync(new Location
            {
                City = model.City.Trim(),
                State = model.State.Trim(),
                Country = model.Country.Trim(),
                Pincode = model.Pincode.Trim(),
                IsActive = model.IsActive
            });

            TempData["Success"] = "Location was added.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Locations/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Edit Location";
            return View(new LocationFormViewModel
            {
                LocationId = location.LocationId,
                City = location.City,
                State = location.State,
                Country = location.Country,
                Pincode = location.Pincode,
                IsActive = location.IsActive
            });
        }

        // POST: /Admin/Locations/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LocationFormViewModel model)
        {
            if (id != model.LocationId)
            {
                return BadRequest();
            }

            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            if (await _locationRepository.ExistsAsync(model.City.Trim(), model.State.Trim(), model.Pincode.Trim(), id))
            {
                ModelState.AddModelError(string.Empty, "Another location with the same city, state and pincode already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Location";
                return View(model);
            }

            location.City = model.City.Trim();
            location.State = model.State.Trim();
            location.Country = model.Country.Trim();
            location.Pincode = model.Pincode.Trim();
            location.IsActive = model.IsActive;

            await _locationRepository.UpdateAsync(location);

            TempData["Success"] = $"Location '{location.DisplayName}' was updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Locations/ToggleActive/5
        [HttpPost("ToggleActive/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            var inUse = await _locationRepository.HasPropertiesAsync(id);

            location.IsActive = !location.IsActive;
            await _locationRepository.UpdateAsync(location);

            if (location.IsActive)
            {
                TempData["Success"] = $"Location '{location.DisplayName}' is active again and can be selected on new listings.";
            }
            else
            {
                TempData["Success"] = inUse
                    ? $"Location '{location.DisplayName}' is now inactive and can no longer be selected. Existing listings keep their location."
                    : $"Location '{location.DisplayName}' is now inactive.";
            }

            _logger.LogInformation("Admin {Admin} set location {LocationId} IsActive={IsActive}.",
                User.Identity?.Name, id, location.IsActive);

            return RedirectToAction(nameof(Index));
        }
    }
}
