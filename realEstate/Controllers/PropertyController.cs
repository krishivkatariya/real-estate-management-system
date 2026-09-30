using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;
using realEstate.Repositories;
using realEstate.Services;
using Microsoft.AspNetCore.Identity;

namespace realEstate.Controllers;

public class PropertyController : Controller
{
    private readonly IPropertyRepository _repo;
    private readonly ApplicationDbContext _db;
    private readonly IImageService _imageService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PropertyController(IPropertyRepository repo, ApplicationDbContext db, IImageService imageService, UserManager<ApplicationUser> userManager)
    {
        _repo = repo;
        _db = db;
        _imageService = imageService;
        _userManager = userManager;
    }

    // Public listing with search, filters, sort and pagination
    public async Task<IActionResult> Index(
        string? search,
        string? city,
        realEstate.Models.PropertyType? propertyType,
        decimal? minPrice,
        decimal? maxPrice,
        int? bedrooms,
        int? bathrooms,
        realEstate.Models.PropertyStatus? status,
        string? sort,
        int page = 1)
    {
        const int pageSize = 6;

        var query = _repo.Query();

        // Keyword search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var kw = search.Trim().ToLower();
            query = query.Where(p => (p.Title != null && p.Title.ToLower().Contains(kw))
                                  || (p.Description != null && p.Description.ToLower().Contains(kw))
                                  || (p.Address != null && p.Address.ToLower().Contains(kw))
                                  || (p.City != null && p.City.ToLower().Contains(kw)));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var c = city.Trim().ToLower();
            query = query.Where(p => p.City != null && p.City.ToLower().Contains(c));
        }

        if (propertyType.HasValue)
        {
            query = query.Where(p => p.PropertyType == propertyType.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (bedrooms.HasValue)
        {
            query = query.Where(p => p.Bedrooms >= bedrooms.Value);
        }

        if (bathrooms.HasValue)
        {
            query = query.Where(p => p.Bathrooms >= bathrooms.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        // Sorting
        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "oldest" => query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt), // newest default
        };

        // Pagination: validate page
        if (page < 1) page = 1;

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (totalPages == 0) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var vm = new realEstate.Models.ViewModels.PropertyListViewModel
        {
            Properties = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Search = search,
            City = city,
            PropertyType = propertyType,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            Status = status,
            Sort = sort
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _repo.GetByIdAsync(id);
        if (property == null) return NotFound();
        return View(property);
    }

    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Property model, List<IFormFile>? images)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        model.OwnerId = user?.Id;
        model.CreatedAt = DateTime.UtcNow;

        await _repo.AddAsync(model);
        await _repo.SaveChangesAsync();

        if (images != null && images.Count > 0)
        {
            foreach (var file in images)
            {
                try
                {
                    await _imageService.SavePropertyImageAsync(file, model.Id);
                }
                catch
                {
                    // ignore individual image errors
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var property = await _repo.GetByIdAsync(id);
        if (property == null) return NotFound();

        if (!await CanEditAsync(property)) return Forbid();

        return View(property);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Property model, List<IFormFile>? images)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var property = await _repo.GetByIdAsync(id);
        if (property == null) return NotFound();
        if (!await CanEditAsync(property)) return Forbid();

        // update allowed fields
        property.Title = model.Title;
        property.Description = model.Description;
        property.Price = model.Price;
        property.Bedrooms = model.Bedrooms;
        property.Bathrooms = model.Bathrooms;
        property.Address = model.Address;
        property.City = model.City;
        property.State = model.State;
        property.ZipCode = model.ZipCode;
        property.Area = model.Area;
        property.PropertyType = model.PropertyType;
        property.Status = model.Status;
        property.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(property);
        await _repo.SaveChangesAsync();

        if (images != null && images.Count > 0)
        {
            foreach (var file in images)
            {
                try
                {
                    await _imageService.SavePropertyImageAsync(file, property.Id);
                }
                catch
                {
                    // ignore
                }
            }
        }

        return RedirectToAction(nameof(Details), new { id = property.Id });
    }

    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var property = await _repo.GetByIdAsync(id);
        if (property == null) return NotFound();
        if (!await CanEditAsync(property)) return Forbid();
        return View(property);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var property = await _repo.GetByIdAsync(id);
        if (property == null) return NotFound();
        if (!await CanEditAsync(property)) return Forbid();

        // delete images files
        foreach (var img in property.Images.ToList())
        {
            await _imageService.DeletePropertyImageAsync(img);
        }

        await _repo.DeleteAsync(property);
        await _repo.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanEditAsync(Property property)
    {
        if (User.IsInRole("Admin")) return true;
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return false;
        return property.OwnerId == user.Id;
    }
}
