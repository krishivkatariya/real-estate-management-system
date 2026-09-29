using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.Models.ViewModels;
using RealEstateManagementSystem.Repositories.Interfaces;

namespace RealEstateManagementSystem.Controllers
{
    /// <summary>
    /// Phase 2 - Admin management of the PropertyType lookup table.
    /// Property types are never deleted (existing listings must stay valid), they are
    /// deactivated with IsActive so they simply disappear from the property form.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("Admin/PropertyTypes")]
    public class AdminPropertyTypesController : Controller
    {
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly ILogger<AdminPropertyTypesController> _logger;

        public AdminPropertyTypesController(
            IPropertyTypeRepository propertyTypeRepository,
            ILogger<AdminPropertyTypesController> logger)
        {
            _propertyTypeRepository = propertyTypeRepository;
            _logger = logger;
        }

        // GET: /Admin/PropertyTypes
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Property Types";

            var propertyTypes = await _propertyTypeRepository.GetAllAsync();
            var items = new List<PropertyTypeListItemViewModel>();

            foreach (var propertyType in propertyTypes)
            {
                items.Add(new PropertyTypeListItemViewModel
                {
                    PropertyType = propertyType,
                    PropertyCount = await _propertyTypeRepository.CountPropertiesAsync(propertyType.PropertyTypeId)
                });
            }

            return View(items);
        }

        // GET: /Admin/PropertyTypes/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Property Type";
            return View(new PropertyTypeFormViewModel());
        }

        // POST: /Admin/PropertyTypes/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyTypeFormViewModel model)
        {
            if (await _propertyTypeRepository.ExistsByNameAsync(model.Name.Trim()))
            {
                ModelState.AddModelError(nameof(model.Name), "A property type with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Add Property Type";
                return View(model);
            }

            await _propertyTypeRepository.AddAsync(new PropertyType
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim(),
                IsActive = model.IsActive
            });

            TempData["Success"] = $"Property type '{model.Name.Trim()}' was added.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/PropertyTypes/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var propertyType = await _propertyTypeRepository.GetByIdAsync(id);
            if (propertyType == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Edit Property Type";
            return View(new PropertyTypeFormViewModel
            {
                PropertyTypeId = propertyType.PropertyTypeId,
                Name = propertyType.Name,
                Description = propertyType.Description,
                IsActive = propertyType.IsActive
            });
        }

        // POST: /Admin/PropertyTypes/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PropertyTypeFormViewModel model)
        {
            if (id != model.PropertyTypeId)
            {
                return BadRequest();
            }

            var propertyType = await _propertyTypeRepository.GetByIdAsync(id);
            if (propertyType == null)
            {
                return NotFound();
            }

            if (await _propertyTypeRepository.ExistsByNameAsync(model.Name.Trim(), id))
            {
                ModelState.AddModelError(nameof(model.Name), "A property type with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Property Type";
                return View(model);
            }

            propertyType.Name = model.Name.Trim();
            propertyType.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            propertyType.IsActive = model.IsActive;

            await _propertyTypeRepository.UpdateAsync(propertyType);

            TempData["Success"] = $"Property type '{propertyType.Name}' was updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/PropertyTypes/ToggleActive/5
        [HttpPost("ToggleActive/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var propertyType = await _propertyTypeRepository.GetByIdAsync(id);
            if (propertyType == null)
            {
                return NotFound();
            }

            var inUse = await _propertyTypeRepository.HasPropertiesAsync(id);

            propertyType.IsActive = !propertyType.IsActive;
            await _propertyTypeRepository.UpdateAsync(propertyType);

            if (propertyType.IsActive)
            {
                TempData["Success"] = $"Property type '{propertyType.Name}' is active again and can be selected on new listings.";
            }
            else
            {
                TempData["Success"] = inUse
                    ? $"Property type '{propertyType.Name}' is now inactive and can no longer be selected. Existing listings keep their type."
                    : $"Property type '{propertyType.Name}' is now inactive.";
            }

            _logger.LogInformation("Admin {Admin} set property type {PropertyTypeId} IsActive={IsActive}.",
                User.Identity?.Name, id, propertyType.IsActive);

            return RedirectToAction(nameof(Index));
        }
    }
}
