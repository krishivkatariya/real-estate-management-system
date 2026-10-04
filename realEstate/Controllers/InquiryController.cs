using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using realEstate.Models;
using realEstate.Models.ViewModels;
using realEstate.Repositories;
using realEstate.Data;
using Microsoft.AspNetCore.Identity;

namespace realEstate.Controllers
{
    [Authorize]
    public class InquiryController : Controller
    {
        private readonly IInquiryRepository _inquiryRepo;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public InquiryController(IInquiryRepository inquiryRepo, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _inquiryRepo = inquiryRepo;
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int propertyId)
        {
            var property = await _db.Properties.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == propertyId);
            if (property == null)
                return NotFound();

            var vm = new InquiryCreateViewModel
            {
                PropertyId = property.Id,
                PropertyTitle = property.Title
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InquiryCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var property = await _db.Properties.FindAsync(model.PropertyId);
            if (property == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Prevent duplicate recent submissions
            if (await _inquiryRepo.ExistsRecentDuplicateAsync(user.Id, model.PropertyId, model.Subject))
            {
                ModelState.AddModelError(string.Empty, "You already submitted a similar inquiry recently. Please wait a few minutes before trying again.");
                return View(model);
            }

            var inquiry = new Inquiry
            {
                PropertyId = model.PropertyId,
                UserId = user.Id,
                Subject = model.Subject,
                Message = model.Message,
                Status = InquiryStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _inquiryRepo.AddAsync(inquiry);

            TempData["SuccessMessage"] = "Your inquiry was sent successfully.";

            return RedirectToAction("MyInquiries");
        }

        public async Task<IActionResult> MyInquiries()
        {
            var user = await _userManager.GetUserAsync(User);
            var inquiries = await _inquiryRepo.GetByUserAsync(user.Id);
            var vm = inquiries.Select(i => new InquiryListViewModel
            {
                Id = i.Id,
                PropertyId = i.PropertyId,
                PropertyTitle = i.Property?.Title ?? "",
                PropertyImageUrl = i.Property?.Images?.FirstOrDefault()?.FilePath ?? "/images/no-image.png",
                Subject = i.Subject,
                CreatedAt = i.CreatedAt,
                Status = i.Status.ToString()
            }).ToList();

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var inquiry = await _inquiryRepo.GetByIdAsync(id);
            if (inquiry == null)
                return NotFound();

            // Authorization: owner of inquiry or property owner or admin
            var isOwner = inquiry.UserId == user.Id;
            var isPropertyOwner = inquiry.Property?.OwnerId == user.Id;
            var isAdmin = User.IsInRole("Admin");
            if (!isOwner && !isPropertyOwner && !isAdmin)
                return Forbid();

            var vm = new InquiryDetailsViewModel
            {
                Id = inquiry.Id,
                PropertyId = inquiry.PropertyId,
                PropertyTitle = inquiry.Property?.Title ?? "",
                PropertyImageUrl = inquiry.Property?.Images?.FirstOrDefault()?.FilePath ?? "/images/no-image.png",
                Subject = inquiry.Subject,
                Message = inquiry.Message,
                CreatedAt = inquiry.CreatedAt,
                Status = inquiry.Status.ToString(),
                ResponseMessage = inquiry.ResponseMessage,
                RespondedAt = inquiry.RespondedAt,
                InquirerEmail = inquiry.User?.Email ?? ""
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ReceivedInquiries()
        {
            var user = await _userManager.GetUserAsync(User);
            var inquiries = await _inquiryRepo.GetByOwnerAsync(user.Id);
            var vm = inquiries.Select(i => new InquiryListViewModel
            {
                Id = i.Id,
                PropertyId = i.PropertyId,
                PropertyTitle = i.Property?.Title ?? "",
                PropertyImageUrl = i.Property?.Images?.FirstOrDefault()?.FilePath ?? "/images/no-image.png",
                Subject = i.Subject,
                CreatedAt = i.CreatedAt,
                Status = i.Status.ToString()
            }).ToList();

            return View(vm);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Manage(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var inquiry = await _inquiryRepo.GetByIdAsync(id);
            if (inquiry == null)
                return NotFound();

            if (inquiry.Property?.OwnerId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            var vm = new InquiryDetailsViewModel
            {
                Id = inquiry.Id,
                PropertyId = inquiry.PropertyId,
                PropertyTitle = inquiry.Property?.Title ?? "",
                PropertyImageUrl = inquiry.Property?.Images?.FirstOrDefault()?.FilePath ?? "/images/no-image.png",
                Subject = inquiry.Subject,
                Message = inquiry.Message,
                CreatedAt = inquiry.CreatedAt,
                Status = inquiry.Status.ToString(),
                ResponseMessage = inquiry.ResponseMessage,
                RespondedAt = inquiry.RespondedAt,
                InquirerEmail = inquiry.User?.Email ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(int id, string? responseMessage, string status)
        {
            var user = await _userManager.GetUserAsync(User);
            var inquiry = await _inquiryRepo.GetByIdAsync(id);
            if (inquiry == null)
                return NotFound();

            if (inquiry.Property?.OwnerId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            if (!string.IsNullOrEmpty(responseMessage))
            {
                inquiry.ResponseMessage = responseMessage;
                inquiry.RespondedAt = DateTime.UtcNow;
            }

            if (Enum.TryParse<InquiryStatus>(status, out var parsedStatus))
            {
                inquiry.Status = parsedStatus;
            }

            await _inquiryRepo.UpdateAsync(inquiry);

            TempData["SuccessMessage"] = "Inquiry updated successfully.";

            return RedirectToAction("ReceivedInquiries");
        }
    }
}
