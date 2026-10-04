using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Controllers;

[Authorize(Roles = "Admin")]
public class AdminApprovalController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminApprovalController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string filter = "All")
    {
        var query = _db.Properties.Include(p => p.Owner).Include(p => p.Images).AsQueryable();
        query = filter switch
        {
            "Pending" => query.Where(p => p.ApprovalStatus == ApprovalStatus.Pending),
            "Approved" => query.Where(p => p.ApprovalStatus == ApprovalStatus.Approved),
            "Rejected" => query.Where(p => p.ApprovalStatus == ApprovalStatus.Rejected),
            _ => query
        };

        var list = await query.OrderByDescending(p => p.SubmittedAt ?? p.CreatedAt).ToListAsync();
        ViewData["Filter"] = filter;
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var prop = await _db.Properties.Include(p => p.Images).Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
        if (prop == null) return NotFound();
        return View(prop);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var prop = await _db.Properties.FirstOrDefaultAsync(p => p.Id == id);
        if (prop == null) return NotFound();
        prop.ApprovalStatus = ApprovalStatus.Approved;
        prop.RejectionReason = null;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Property approved successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var prop = await _db.Properties.FirstOrDefaultAsync(p => p.Id == id);
        if (prop == null) return NotFound();
        prop.ApprovalStatus = ApprovalStatus.Rejected;
        prop.RejectionReason = reason;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Property rejected.";
        return RedirectToAction("Index");
    }
}
