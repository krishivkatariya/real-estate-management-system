using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using realEstate.Data;
using realEstate.Models;

namespace realEstate.Controllers;

[Authorize(Roles = "Admin")]
public class AdminManagementController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminManagementController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // List users with optional role filter and search
    public async Task<IActionResult> Users(string? role, string? q)
    {
        var usersQuery = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.Trim().ToLower();
            usersQuery = usersQuery.Where(u => u.UserName.ToLower().Contains(ql) || u.Email.ToLower().Contains(ql));
        }

        // if role filter provided, join with userroles
        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleEntity = await _db.Roles.FirstOrDefaultAsync(r => r.Name == role);
            if (roleEntity != null)
            {
                var userIds = await _db.UserRoles.Where(ur => ur.RoleId == roleEntity.Id).Select(ur => ur.UserId).ToListAsync();
                usersQuery = usersQuery.Where(u => userIds.Contains(u.Id));
            }
        }

        var users = await usersQuery.OrderBy(u => u.UserName).ToListAsync();

        var vm = new List<UserListItemViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            vm.Add(new UserListItemViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Roles = string.Join(", ", roles),
                RegistrationDate = null,
                Status = (u.LockoutEnd.HasValue && u.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow) ? "Locked" : "Active"
            });
        }

        ViewData["RoleFilter"] = role;
        ViewData["Search"] = q;
        return View(vm);
    }

    public async Task<IActionResult> UserDetails(string id)
    {
        if (string.IsNullOrEmpty(id)) return BadRequest();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = roles,
            RegistrationDate = null
        };
        return View(model);
    }
}

public class UserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public DateTime? RegistrationDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UserDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    public DateTime? RegistrationDate { get; set; }
}
