using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using realEstate.Models;
using realEstate.Repositories;
using Microsoft.AspNetCore.Identity;
using realEstate.Data;
using System.ComponentModel.DataAnnotations;

namespace realEstate.Controllers;

[Authorize(Roles = "Buyer,Seller,Admin")]
public class BookingRequestController : Controller
{
    private readonly IBookingRequestRepository _repo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public BookingRequestController(IBookingRequestRepository repo, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _repo = repo;
        _userManager = userManager;
        _db = db;
    }

    // GET: show create form for buyers
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Create(int propertyId)
    {
        var property = await _db.Properties.FindAsync(propertyId);
        if (property == null || property.ApprovalStatus != ApprovalStatus.Approved) return NotFound();

        var vm = new BookingCreateViewModel
        {
            PropertyId = property.Id,
            PropertyTitle = property.Title,
            Price = property.Price
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var property = await _db.Properties.FindAsync(model.PropertyId);
        if (property == null || property.ApprovalStatus != ApprovalStatus.Approved) return BadRequest();

        if (string.IsNullOrEmpty(property.OwnerId)) return BadRequest("Property is not assigned to a seller.");

        var request = new BookingRequest
        {
            BuyerId = user.Id,
            PropertyId = property.Id,
            SellerId = property.OwnerId,
            PreferredDate = model.PreferredDate,
            PreferredTime = model.PreferredTime,
            Message = model.Message,
            Status = BookingStatus.Pending
        };

        await _repo.AddAsync(request);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your booking request has been sent to the seller and is waiting for approval.";
        return RedirectToAction("Buyer", "Dashboard");
    }

    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> MyRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        var list = await _repo.GetByBuyerAsync(user.Id);
        return View(list);
    }

    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Received()
    {
        var user = await _userManager.GetUserAsync(User);
        var list = await _repo.GetBySellerAsync(user.Id);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Approve(int id, string? response)
    {
        var req = await _repo.GetByIdAsync(id);
        if (req == null) return NotFound();
        var user = await _userManager.GetUserAsync(User);
        if (user == null || req.SellerId != user.Id) return Forbid();

        req.Status = BookingStatus.Approved;
        req.SellerResponse = response;
        req.RespondedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(req);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Booking request approved.";
        // After approving, redirect seller back to their dashboard
        return RedirectToAction("Seller", "Dashboard");
    }

    public async Task<IActionResult> Details(int id)
    {
        var req = await _repo.GetByIdAsync(id);
        if (req == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        var isBuyer = user != null && req.BuyerId == user.Id;
        var isSeller = user != null && req.SellerId == user.Id;
        var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

        if (!isBuyer && !isSeller && !isAdmin) return Forbid();

        return View(req);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Cancel(int id)
    {
        var req = await _repo.GetByIdAsync(id);
        if (req == null) return NotFound();
        var user = await _userManager.GetUserAsync(User);
        if (user == null || req.BuyerId != user.Id) return Forbid();

        if (req.Status != BookingStatus.Pending)
        {
            TempData["Error"] = "Only pending requests can be cancelled.";
            return RedirectToAction("MyRequests");
        }

        req.Status = BookingStatus.Cancelled;
        req.SellerResponse = "Cancelled by buyer";
        req.RespondedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(req);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Booking request cancelled.";
        return RedirectToAction("MyRequests");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var req = await _repo.GetByIdAsync(id);
        if (req == null) return NotFound();
        var user = await _userManager.GetUserAsync(User);
        if (user == null || req.SellerId != user.Id) return Forbid();

        req.Status = BookingStatus.Rejected;
        req.SellerResponse = reason;
        req.RespondedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(req);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Booking request rejected.";
        return RedirectToAction("Received");
    }
}

public class BookingCreateViewModel
{
    public int PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }

    [Required]
    public DateTime PreferredDate { get; set; }

    [Required]
    public string PreferredTime { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Message { get; set; }
}
