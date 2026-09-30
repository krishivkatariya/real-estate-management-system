using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using realEstate.Models;
using realEstate.Repositories;
using Microsoft.AspNetCore.Identity;

namespace realEstate.Controllers;

[Authorize]
public class PurchaseRequestsController : Controller
{
    private readonly IPurchaseRequestRepository _requests;
    private readonly UserManager<ApplicationUser> _userManager;

    public PurchaseRequestsController(IPurchaseRequestRepository requests, UserManager<ApplicationUser> userManager)
    {
        _requests = requests;
        _userManager = userManager;
    }

    // Buyer: view their requests
    public async Task<IActionResult> MyRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var list = await _requests.GetRequestsByBuyerAsync(user.Id);
        return View(list);
    }

    // Owner: view requests for their properties
    public async Task<IActionResult> ForMyProperties()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var list = await _requests.GetRequestsForOwnerAsync(user.Id);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int propertyId, string? message)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // validate property
        var db = HttpContext.RequestServices.GetService(typeof(realEstate.Data.ApplicationDbContext)) as realEstate.Data.ApplicationDbContext;
        var property = await db!.Properties.FindAsync(propertyId);
        if (property == null) return NotFound();
        if (property.OwnerId == user.Id) return BadRequest("Cannot request purchase for your own property.");
        if (property.Status != PropertyStatus.Available) return BadRequest("Property not available for sale.");

        // prevent duplicate pending requests
        if (await _requests.ExistsOpenRequestAsync(user.Id, propertyId))
        {
            TempData["PurchaseMessage"] = "You already have a pending request for this property.";
            return RedirectToAction("Details", "Property", new { id = propertyId });
        }

        var req = new PurchaseRequest
        {
            UserId = user.Id,
            PropertyId = propertyId,
            Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim(),
            RequestDate = DateTime.UtcNow,
            Status = PurchaseRequestStatus.Pending
        };

        await _requests.AddAsync(req);
        await _requests.SaveChangesAsync();

        TempData["PurchaseMessage"] = "Your purchase request has been submitted.";
        return RedirectToAction("Details", "Property", new { id = propertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int requestId, PurchaseRequestStatus status)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var request = await _requests.GetByIdAsync(requestId);
        if (request == null) return NotFound();

        // authorization: only owner of property or admin
        var isOwner = request.Property?.OwnerId == user.Id;
        if (!isOwner && !User.IsInRole("Admin")) return Forbid();

        // cannot change to Pending
        if (status == PurchaseRequestStatus.Pending) return BadRequest();

        request.Status = status;
        await _requests.UpdateAsync(request);
        await _requests.SaveChangesAsync();

        return RedirectToAction("ForMyProperties");
    }
}
