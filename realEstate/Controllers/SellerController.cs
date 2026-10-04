using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using realEstate.Data;
using realEstate.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace realEstate.Controllers;

[Authorize(Roles = "Seller,Admin")]
public class SellerController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SellerController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var total = await _db.Properties.CountAsync(p => p.OwnerId == user.Id);
        var pending = await _db.Properties.CountAsync(p => p.OwnerId == user.Id && p.ApprovalStatus == ApprovalStatus.Pending);
        var approved = await _db.Properties.CountAsync(p => p.OwnerId == user.Id && p.ApprovalStatus == ApprovalStatus.Approved);
        var rejected = await _db.Properties.CountAsync(p => p.OwnerId == user.Id && p.ApprovalStatus == ApprovalStatus.Rejected);

        ViewData["Total"] = total;
        ViewData["Pending"] = pending;
        ViewData["Approved"] = approved;
        ViewData["Rejected"] = rejected;

        // Provide the seller's properties as the view model so the Razor can render the list
        var list = await _db.Properties
            .Where(p => p.OwnerId == user.Id)
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(list);
    }

    public IActionResult Create()
    {
        return View(new Property());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Property model, List<IFormFile>? images, string[]? amenities)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        model.OwnerId = user.Id;
        model.ApprovalStatus = ApprovalStatus.Pending;
        model.SubmittedAt = DateTime.UtcNow;
        model.CreatedAt = DateTime.UtcNow;
        model.Amenities = amenities != null ? string.Join(',', amenities) : null;

        await _db.Properties.AddAsync(model);
        await _db.SaveChangesAsync();

        // handle image uploads using registered IImageService if available
        try
        {
            var imageService = HttpContext.RequestServices.GetService(typeof(realEstate.Services.IImageService)) as realEstate.Services.IImageService;
            if (imageService != null && images != null && images.Count > 0)
            {
                foreach (var file in images)
                {
                    try
                    {
                        await imageService.SavePropertyImageAsync(file, model.Id);
                    }
                    catch { /* don't fail entire request on image errors */ }
                }
            }
        }
        catch { }

        // handle images using existing image service via PropertyController logic if available
        TempData["SuccessMessage"] = "Property submitted for approval.";
        return RedirectToAction("Dashboard");
    }

    public async Task<IActionResult> MyProperties()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var list = await _db.Properties.Where(p => p.OwnerId == user.Id).OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var prop = await _db.Properties.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (prop == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (prop.OwnerId != user.Id && !User.IsInRole("Admin")) return Forbid();

        return View(prop);
    }
}
