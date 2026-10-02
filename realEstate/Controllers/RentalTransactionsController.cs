using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;
using realEstate.Models.ViewModels;
using realEstate.Repositories;
using System.Data;

namespace realEstate.Controllers;

[Authorize]
public class RentalTransactionsController : Controller
{
    private readonly IRentalTransactionRepository _rentals;
    private readonly IPropertyRepository _properties;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public RentalTransactionsController(
        IRentalTransactionRepository rentals,
        IPropertyRepository properties,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db)
    {
        _rentals = rentals;
        _properties = properties;
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int propertyId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var property = await _properties.GetByIdAsync(propertyId);
        if (property == null) return NotFound();
        if (!CanRequestRental(property, user.Id)) return BadRequest("This property is not currently available to rent.");
        if (await _rentals.HasOpenRequestAsync(user.Id, propertyId))
        {
            return BadRequest("You already have an open rental request for this property.");
        }

        return View(CreateRequestModel(property, propertyId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RentalRequestViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!ModelState.IsValid)
        {
            var displayProperty = await _properties.GetByIdAsync(model.PropertyId);
            if (displayProperty == null) return NotFound();
            model.PropertyTitle = displayProperty.Title;
            model.MonthlyRent = displayProperty.Price;
            return View(model);
        }

        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            var property = await _db.Properties
                .FromSqlInterpolated($"SELECT * FROM [Properties] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {model.PropertyId}")
                .FirstOrDefaultAsync();
            if (property == null) return NotFound();

            model.PropertyTitle = property.Title;
            model.MonthlyRent = property.Price;
            if (!CanRequestRental(property, user.Id))
            {
                ModelState.AddModelError(string.Empty, "This property is not currently available to rent.");
            }

            if (property.Price <= 0)
            {
                ModelState.AddModelError(string.Empty, "The property's monthly rent must be greater than zero.");
            }

            if (await _rentals.HasOpenRequestAsync(user.Id, property.Id))
            {
                ModelState.AddModelError(string.Empty, "You already have an open rental request for this property.");
            }

            var startDate = AsUtcDate(model.StartDate);
            var endDate = AsUtcDate(model.EndDate);
            if (ModelState.IsValid && await _rentals.HasOverlappingOpenRentalAsync(property.Id, startDate, endDate))
            {
                ModelState.AddModelError(string.Empty, "The selected dates overlap an open rental request or rental.");
            }

            if (!ModelState.IsValid) return View(model);

            await _rentals.AddAsync(new RentalTransaction
            {
                PropertyId = property.Id,
                RenterId = user.Id,
                OwnerId = property.OwnerId!,
                StartDate = startDate,
                EndDate = endDate,
                RentAmount = property.Price,
                Status = RentalStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim()
            });
            await _rentals.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception exception) when (IsRentalConcurrencyException(exception))
        {
            var property = await _properties.GetByIdAsync(model.PropertyId);
            if (property != null)
            {
                model.PropertyTitle = property.Title;
                model.MonthlyRent = property.Price;
            }
            ModelState.AddModelError(string.Empty, "A conflicting rental request was submitted at the same time. Refresh and try again.");
            return View(model);
        }

        TempData["RentalMessage"] = "Your rental request has been sent to the property owner.";
        TempData["RentalMessageType"] = "success";
        return RedirectToAction(nameof(MyRequests));
    }

    [HttpGet]
    public async Task<IActionResult> MyRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        return View(await _rentals.GetByRenterIdAsync(user.Id));
    }

    [HttpGet]
    public async Task<IActionResult> OwnerRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var rentals = User.IsInRole("Admin")
            ? await _rentals.GetAllAsync()
            : await _rentals.GetByOwnerIdAsync(user.Id);
        return View(rentals);
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var rentals = User.IsInRole("Admin")
            ? await _rentals.GetAllAsync()
            : await _rentals.GetHistoryForUserAsync(user.Id);
        return View(rentals);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var rental = await _rentals.GetByIdAsync(id);
        if (rental == null) return NotFound();
        if (!CanViewRental(rental, user.Id)) return Forbid();

        return View(rental);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var rental = await _rentals.GetByIdAsync(id);
        if (rental == null) return NotFound();
        if (!await CanManageRentalAsync(rental)) return Forbid();
        if (rental.Status != RentalStatus.Pending)
        {
            return RentalError("Only pending rental requests can be approved.", nameof(OwnerRequests));
        }
        if (rental.Property == null || !CanApproveForProperty(rental, rental.Property))
        {
            return RentalError("This property is no longer eligible for rental.", nameof(OwnerRequests));
        }
        if (await _rentals.HasOverlappingOpenRentalAsync(rental.PropertyId, rental.StartDate, rental.EndDate, rental.RentalTransactionId))
        {
            return RentalError("Another approved or active rental overlaps these dates.", nameof(OwnerRequests));
        }

        rental.Status = RentalStatus.Approved;
        rental.ApprovedAt = DateTime.UtcNow;
        await _rentals.UpdateAsync(rental);
        await _rentals.SaveChangesAsync();
        return RentalSuccess("Rental request approved.", nameof(OwnerRequests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var rental = await _rentals.GetByIdAsync(id);
        if (rental == null) return NotFound();
        if (!await CanManageRentalAsync(rental)) return Forbid();
        if (rental.Status != RentalStatus.Pending)
        {
            return RentalError("Only pending rental requests can be rejected.", nameof(OwnerRequests));
        }

        rental.Status = RentalStatus.Rejected;
        rental.RejectedAt = DateTime.UtcNow;
        await _rentals.UpdateAsync(rental);
        await _rentals.SaveChangesAsync();
        return RentalSuccess("Rental request rejected.", nameof(OwnerRequests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var rental = await _rentals.GetByIdAsync(id);
        if (rental == null) return NotFound();
        if (!await CanManageRentalAsync(rental)) return Forbid();
        if (rental.Status != RentalStatus.Approved || rental.Property == null
            || !CanApproveForProperty(rental, rental.Property))
        {
            return RentalError("Only an approved rental for an available Rent listing can be activated.", nameof(OwnerRequests));
        }

        var today = DateTime.UtcNow.Date;
        if (rental.StartDate.Date > today || rental.EndDate.Date <= today)
        {
            return RentalError("A rental can be activated only during its approved date range.", nameof(OwnerRequests));
        }
        if (await _rentals.HasOverlappingOpenRentalAsync(rental.PropertyId, rental.StartDate, rental.EndDate, rental.RentalTransactionId))
        {
            return RentalError("Another approved or active rental overlaps these dates.", nameof(OwnerRequests));
        }

        rental.Status = RentalStatus.Active;
        rental.Property.Status = PropertyStatus.Rented;
        await _rentals.UpdateAsync(rental);
        await _rentals.SaveChangesAsync();
        return RentalSuccess("Rental activated; the property is now marked Rented.", nameof(OwnerRequests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var rental = await _rentals.GetByIdAsync(id);
        if (rental == null) return NotFound();
        if (!await CanManageRentalAsync(rental)) return Forbid();
        if (rental.Status != RentalStatus.Active || rental.Property == null)
        {
            return RentalError("Only active rentals can be completed.", nameof(OwnerRequests));
        }

        rental.Status = RentalStatus.Completed;
        if (rental.Property.Status == PropertyStatus.Rented
            && rental.Property.ListingPurpose == ListingPurpose.Rent)
        {
            rental.Property.Status = PropertyStatus.Available;
        }

        await _rentals.UpdateAsync(rental);
        await _rentals.SaveChangesAsync();
        return RentalSuccess("Rental completed; the Rent listing is available again.", nameof(OwnerRequests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var rental = await _rentals.GetByIdAsync(id);
        if (rental == null) return NotFound();
        var canManage = await CanManageRentalAsync(rental);
        var isRenter = rental.RenterId == user.Id;
        if ((!isRenter && !canManage)
            || (rental.Status == RentalStatus.Active && !canManage)
            || (rental.Status != RentalStatus.Pending && rental.Status != RentalStatus.Approved && rental.Status != RentalStatus.Active))
        {
            return Forbid();
        }

        rental.Status = RentalStatus.Cancelled;
        if (rental.Property?.Status == PropertyStatus.Rented
            && rental.Property.ListingPurpose == ListingPurpose.Rent)
        {
            rental.Property.Status = PropertyStatus.Available;
        }

        await _rentals.UpdateAsync(rental);
        await _rentals.SaveChangesAsync();
        return RentalSuccess("Rental request cancelled.", nameof(MyRequests));
    }

    private static RentalRequestViewModel CreateRequestModel(Property property, int propertyId)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        return new RentalRequestViewModel
        {
            PropertyId = propertyId,
            PropertyTitle = property.Title,
            MonthlyRent = property.Price,
            StartDate = startDate,
            EndDate = startDate.AddMonths(1)
        };
    }

    private static bool CanRequestRental(Property property, string userId)
    {
        return property.ListingPurpose == ListingPurpose.Rent
            && property.Status == PropertyStatus.Available
            && !string.IsNullOrEmpty(property.OwnerId)
            && property.OwnerId != userId;
    }

    private static bool CanApproveForProperty(RentalTransaction rental, Property property)
    {
        return property.ListingPurpose == ListingPurpose.Rent
            && property.Status == PropertyStatus.Available
            && property.OwnerId == rental.OwnerId;
    }

    private bool CanViewRental(RentalTransaction rental, string userId)
    {
        return User.IsInRole("Admin") || rental.RenterId == userId || rental.OwnerId == userId;
    }

    private async Task<bool> CanManageRentalAsync(RentalTransaction rental)
    {
        if (User.IsInRole("Admin")) return true;
        var user = await _userManager.GetUserAsync(User);
        return user != null
            && rental.OwnerId == user.Id
            && rental.Property?.OwnerId == user.Id;
    }

    private IActionResult RentalError(string message, string action)
    {
        TempData["RentalMessage"] = message;
        TempData["RentalMessageType"] = "danger";
        return RedirectToAction(action);
    }

    private IActionResult RentalSuccess(string message, string action)
    {
        TempData["RentalMessage"] = message;
        TempData["RentalMessageType"] = "success";
        return RedirectToAction(action);
    }

    private static DateTime AsUtcDate(DateTime value)
    {
        return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    }

    private static bool IsRentalConcurrencyException(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException!)
        {
            if (current is SqlException sqlException
                && (sqlException.Number == 1205
                    || sqlException.Number == 1222
                    || sqlException.Number == 2601
                    || sqlException.Number == 2627))
            {
                return true;
            }
        }

        return false;
    }
}