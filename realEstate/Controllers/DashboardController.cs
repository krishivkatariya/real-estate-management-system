using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using realEstate.Data;
using Microsoft.AspNetCore.Identity;
using realEstate.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace realEstate.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [Authorize(Roles = "Buyer,Seller,Admin")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (await _userManager.IsInRoleAsync(user, "Admin")) return RedirectToAction("Admin");
        if (await _userManager.IsInRoleAsync(user, "Seller")) return RedirectToAction("Seller");
        return RedirectToAction("Buyer");
    }

    [Authorize(Roles = "Buyer,Seller,Admin")]
    public async Task<IActionResult> Buyer()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var favoritesCount = await _db.Favorites.CountAsync(f => f.UserId == user.Id);
        var purchaseCount = await _db.PurchaseRequests.CountAsync(p => p.UserId == user.Id);
        var rentalCount = await _db.RentalTransactions.CountAsync(r => r.RenterId == user.Id);
        var inquiriesCount = await _db.Inquiries.CountAsync(i => i.UserId == user.Id);

        var recentInquiries = await _db.Inquiries
            .Where(i => i.UserId == user.Id)
            .Include(i => i.Property)
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewData["Title"] = "Buyer Dashboard";
        ViewData["Welcome"] = user.UserName;
        ViewData["FavoritesCount"] = favoritesCount;
        ViewData["PurchaseCount"] = purchaseCount;
        ViewData["RentalCount"] = rentalCount;
        ViewData["InquiriesCount"] = inquiriesCount;
        ViewData["RecentInquiries"] = recentInquiries;

        return View();
    }

    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> Seller()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var totalListed = await _db.Properties.CountAsync(p => p.OwnerId == user.Id);
        var available = await _db.Properties.CountAsync(p => p.OwnerId == user.Id && p.Status == PropertyStatus.Available);
        // Treat 'Accepted' purchase requests as sold
        var sold = await _db.PurchaseRequests.CountAsync(pr => pr.Property.OwnerId == user.Id && pr.Status == realEstate.Models.PurchaseRequestStatus.Accepted);
        var rented = await _db.RentalTransactions.CountAsync(r => r.OwnerId == user.Id && r.Status == realEstate.Models.RentalStatus.Active);
        var totalInquiries = await _db.Inquiries.CountAsync(i => i.Property.OwnerId == user.Id);
        var pendingInquiries = await _db.Inquiries.CountAsync(i => i.Property.OwnerId == user.Id && i.Status == InquiryStatus.Pending);

        ViewData["Title"] = "Seller Dashboard";
        ViewData["Welcome"] = user.UserName;
        ViewData["TotalListed"] = totalListed;
        ViewData["Available"] = available;
        ViewData["Sold"] = sold;
        ViewData["Rented"] = rented;
        ViewData["TotalInquiries"] = totalInquiries;
        ViewData["PendingInquiries"] = pendingInquiries;

        return View();
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin()
    {
        var totalUsers = await _db.Users.CountAsync();
        var totalBuyers = await _db.Users.CountAsync(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Buyer")));
        var totalSellers = await _db.Users.CountAsync(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Seller")));
        var totalProperties = await _db.Properties.CountAsync();
        var availableProperties = await _db.Properties.CountAsync(p => p.Status == PropertyStatus.Available);
        var soldProperties = await _db.PurchaseRequests.CountAsync(pr => pr.Status == realEstate.Models.PurchaseRequestStatus.Accepted);
        var totalInquiries = await _db.Inquiries.CountAsync();
        var pendingInquiries = await _db.Inquiries.CountAsync(i => i.Status == InquiryStatus.Pending);

        var approvedProperties = await _db.Properties.CountAsync(p => p.ApprovalStatus == ApprovalStatus.Approved);
        var pendingProperties = await _db.Properties.CountAsync(p => p.ApprovalStatus == ApprovalStatus.Pending);
        var rejectedProperties = await _db.Properties.CountAsync(p => p.ApprovalStatus == ApprovalStatus.Rejected);

        ViewData["Title"] = "Admin Dashboard";
        ViewData["TotalUsers"] = totalUsers;
        ViewData["TotalBuyers"] = totalBuyers;
        ViewData["TotalSellers"] = totalSellers;
        ViewData["TotalProperties"] = totalProperties;
        ViewData["AvailableProperties"] = availableProperties;
        ViewData["SoldProperties"] = soldProperties;
        ViewData["TotalInquiries"] = totalInquiries;
        ViewData["PendingInquiries"] = pendingInquiries;
        ViewData["ApprovedProperties"] = approvedProperties;
        ViewData["PendingProperties"] = pendingProperties;
        ViewData["RejectedProperties"] = rejectedProperties;

        return View();
    }
}
